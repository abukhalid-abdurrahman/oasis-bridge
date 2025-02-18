using NetworkType = Domain.Enums.NetworkType;
using PrivateKey = RadixEngineToolkit.PrivateKey;
using PublicKey = RadixEngineToolkit.PublicKey;

namespace Infrastructure.ImplementationContract;

public sealed class OrderService(
    DataContext dbContext,
    IHttpContextAccessor accessor,
    ISolanaBridge solanaBridge,
    IRadixBridge radixBridge,
    RadixTechnicalAccountBridgeOptions radixOp) : IOrderService
{
    private const string Solana = "Solana";
    private const string Radix = "Radix";

    private const string Sol = "SOL";
    private const string Xrd = "XRD";


    public async Task<Result<CreateOrderResponse>> CreateOrderAsync(CreateOrderRequest request,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        bool userExists = await dbContext.Users.AnyAsync(x => x.Id == request.UserId, token);
        bool fromNetworkExists = await dbContext.Networks.AnyAsync(x => x.Name == request.FromNetwork, token);
        bool toNetworkExists = await dbContext.Networks.AnyAsync(x => x.Name == request.ToNetwork, token);
        bool fromTokenExists = await dbContext.NetworkTokens.AnyAsync(x => x.Symbol == request.FromToken, token);
        bool toTokenExists = await dbContext.NetworkTokens.AnyAsync(x => x.Symbol == request.ToToken, token);

        if (!(userExists && fromNetworkExists && toNetworkExists && fromTokenExists && toTokenExists))
            return Result<CreateOrderResponse>.Failure(ResultPatternError.NotFound("Invalid input data"));

        VirtualAccount? virtualAccount = await dbContext.VirtualAccounts
            .Include(x => x.Network)
            .FirstOrDefaultAsync(x =>
                x.UserId == request.UserId && x.Network.Name == request.FromNetwork, token);

        ExchangeRate exchangeRate = await dbContext.ExchangeRates
            .Include(x => x.FromToken)
            .Include(x => x.ToToken)
            .Where(x => x.FromToken.Symbol == request.FromToken && x.ToToken.Symbol == request.ToToken)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(token) ?? new();

        decimal convertedAmount = exchangeRate.Rate * request.Amount;

        if (virtualAccount == null)
        {
            virtualAccount = await CreateVirtualAccountAsync(request, token);
            if (virtualAccount == null)
                return Result<CreateOrderResponse>.Failure(
                    ResultPatternError.InternalServerError("Couldn't create virtual account"));

            await dbContext.VirtualAccounts.AddAsync(virtualAccount, token);
            int res = await dbContext.SaveChangesAsync(token);
            if (res == 0)
                return Result<CreateOrderResponse>.Failure(
                    ResultPatternError.InternalServerError("Couldn't create virtual account"));

            Order newOrder = new Order
            {
                UserId = request.UserId,
                CreatedBy = accessor.GetId(),
                CreatedByIp = accessor.GetRemoteIpAddress(),
                FromToken = request.FromToken,
                ToToken = request.ToToken,
                FromNetwork = request.FromNetwork,
                ToNetwork = request.ToNetwork,
                DestinationAddress = request.DestinationAddress,
                OrderStatus = OrderStatus.InsufficientFunds,
                ExchangeRateId = exchangeRate.Id,
                Amount = request.Amount,
            };

            await dbContext.Orders.AddAsync(newOrder, token);
            int response = await dbContext.SaveChangesAsync(token);

            return response != 0
                ? Result<CreateOrderResponse>.Success(new CreateOrderResponse(newOrder.Id, "Order created"))
                : Result<CreateOrderResponse>.Failure(ResultPatternError.InternalServerError("Error creating order"));
        }

        if (request is { FromToken: Xrd, ToToken: Sol } ||
            request is { FromToken: Sol, ToToken: Xrd })
        {
            IBridge withdrawBridge = request.FromToken == Xrd ? radixBridge : solanaBridge;
            IBridge depositBridge = request.FromToken == Xrd ? solanaBridge : radixBridge;

            decimal balance = await withdrawBridge.GetAccountBalanceAsync(virtualAccount.Address, token);
            if (balance - 5 <= request.Amount)
                return Result<CreateOrderResponse>.Failure(
                    ResultPatternError.BadRequest("Insufficient balance"));

            TransactionResponse withdrawTrRs =
                await withdrawBridge.WithdrawAsync(request.Amount, virtualAccount.Address, virtualAccount.PrivateKey);
            if (!withdrawTrRs.Success)
                throw new Exception("Error sending transaction");

            TransactionResponse depositTrRs =
                await depositBridge.DepositAsync(convertedAmount, request.DestinationAddress);
            if (!depositTrRs.Success)
            {
                TransactionResponse abortTrRs =
                    await withdrawBridge.DepositAsync(request.Amount, virtualAccount.Address);
                if (!abortTrRs.Success) throw new Exception("Error sending transaction");
                return Result<CreateOrderResponse>.Failure(
                    ResultPatternError.InternalServerError("Transaction failed, rollback attempted"));
            }

            BridgeTransactionStatus transactionStatus =
                await depositBridge.GetTransactionStatusAsync(depositTrRs.TransactionId!, token);
            OrderStatus orderStatus = transactionStatus == BridgeTransactionStatus.Succeed
                ? OrderStatus.Completed
                : OrderStatus.Canceled;

            var newOrder = new Order
            {
                UserId = request.UserId,
                CreatedBy = accessor.GetId(),
                CreatedByIp = accessor.GetRemoteIpAddress(),
                FromToken = request.FromToken,
                ToToken = request.ToToken,
                FromNetwork = request.FromNetwork,
                ToNetwork = request.ToNetwork,
                DestinationAddress = request.DestinationAddress,
                OrderStatus = orderStatus,
                ExchangeRateId = exchangeRate.Id,
                Amount = request.Amount
            };

            await dbContext.Orders.AddAsync(newOrder, token);
            int res = await dbContext.SaveChangesAsync(token);

            return res > 0
                ? Result<CreateOrderResponse>.Success(new CreateOrderResponse(newOrder.Id, "Order created"))
                : Result<CreateOrderResponse>.Failure(ResultPatternError.InternalServerError("Could not create order"));
        }

        return Result<CreateOrderResponse>.Failure(ResultPatternError.BadRequest("Unsupported token pair"));
    }


    private async Task<VirtualAccount?> CreateVirtualAccountAsync(CreateOrderRequest request, CancellationToken token)
    {
        VirtualAccount newVirtualAccount = new()
        {
            UserId = request.UserId,
            CreatedBy = HttpAccessor.SystemId,
            CreatedByIp = accessor.GetRemoteIpAddress()
        };

        if (request.FromNetwork == Solana)
        {
            var (publicKey, privateKey, seedPhrase) = await solanaBridge.CreateAccountAsync(token);
            newVirtualAccount = await PopulateSolanaAccountAsync(publicKey, privateKey, seedPhrase, token);
        }
        else if (request.FromNetwork == Radix)
        {
            var (publicKey, privateKey, seedPhrase) = radixBridge.CreateAccountAsync(token);
            newVirtualAccount = await PopulateRadixAccountAsync(publicKey, privateKey, seedPhrase, token);
        }

        return newVirtualAccount;
    }

    private async Task<VirtualAccount> PopulateSolanaAccountAsync(string publicKey, string privateKey,
        string seedPhrase, CancellationToken token)
    {
        return new VirtualAccount
        {
            NetworkId = await dbContext.Networks.Where(x => x.Name == Solana).Select(x => x.Id)
                .FirstOrDefaultAsync(token),
            PublicKey = publicKey,
            PrivateKey = privateKey,
            SeedPhrase = seedPhrase,
            Address = publicKey,
            NetworkType = NetworkType.Solana
        };
    }

    private async Task<VirtualAccount> PopulateRadixAccountAsync(PublicKey publicKey, PrivateKey privateKey,
        string seedPhrase, CancellationToken token)
    {
        return new VirtualAccount
        {
            NetworkId = await dbContext.Networks.Where(x => x.Name == Radix).Select(x => x.Id)
                .FirstOrDefaultAsync(token),
            PublicKey = Encoders.Hex.EncodeData(privateKey.PublicKeyBytes()),
            PrivateKey = privateKey.RawHex(),
            SeedPhrase = seedPhrase,
            Address = radixBridge.GetAddressAsync(publicKey, AddressType.Account,
                radixOp.NetworkId == 0x01 ? RadixBridge.Enums.NetworkType.Main : RadixBridge.Enums.NetworkType.Test,
                token),
            NetworkType = NetworkType.Radix
        };
    }

    public async Task<Result<CheckBalanceResponse>> CheckBalanceAsync(Guid orderId, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        Order? order = await dbContext.Orders.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == orderId, token);
        if (order is null)
            return Result<CheckBalanceResponse>.Failure(ResultPatternError.NotFound("Order not found"));

        IBridge bridge = order.FromNetwork == Solana ? solanaBridge : radixBridge;

        VirtualAccount? virtualAccount = await dbContext.VirtualAccounts
            .FirstOrDefaultAsync(x => x.UserId == order.UserId && x.Network.Name == order.FromNetwork, token);
        if (virtualAccount is null)
            return Result<CheckBalanceResponse>.Failure(ResultPatternError.NotFound("Virtual account not found"));

        decimal balance = await bridge.GetAccountBalanceAsync(virtualAccount.Address, token);

        if (order.OrderStatus == OrderStatus.Completed)
            return Result<CheckBalanceResponse>.Success(new CheckBalanceResponse(order.Id, order.FromNetwork,
                order.FromToken, balance, order.Amount, "completed", "Order is already completed."));

        bool isExpired = (DateTimeOffset.UtcNow - order.CreatedAt).TotalMinutes >= 10;
        if (isExpired)
        {
            await UpdateOrderStatusAsync(order.Id, OrderStatus.Canceled, token);
            return Result<CheckBalanceResponse>.Failure(
                ResultPatternError.BadRequest("The replenishment timeout has expired. The order has been rejected."));
        }

        if (balance < order.Amount + 5)
        {
            return Result<CheckBalanceResponse>.Success(new CheckBalanceResponse(order.Id, order.FromNetwork,
                order.FromToken, balance, order.Amount, "insufficient_funds",
                "Insufficient funds. Waiting for replenishment."));
        }

        return await ProcessTransactionAsync(order, virtualAccount, token);
    }


    private async Task<Result<CheckBalanceResponse>> ProcessTransactionAsync(Order order, VirtualAccount virtualAccount,
        CancellationToken token)
    {
        IBridge withdrawBridge = order.FromToken == Xrd ? radixBridge : solanaBridge;
        IBridge depositBridge = order.FromToken == Xrd ? solanaBridge : radixBridge;

        TransactionResponse withdrawTrRs =
            await withdrawBridge.WithdrawAsync(order.Amount, virtualAccount.Address, virtualAccount.PrivateKey);
        if (!withdrawTrRs.Success)
            return Result<CheckBalanceResponse>.Failure(
                ResultPatternError.InternalServerError("Error"));

        TransactionResponse depositTrRs = await depositBridge.DepositAsync(order.Amount, order.DestinationAddress);
        if (!depositTrRs.Success)
        {
            await withdrawBridge.DepositAsync(order.Amount, virtualAccount.Address);
            return Result<CheckBalanceResponse>.Failure(
                ResultPatternError.InternalServerError("Error processing"));
        }

        BridgeTransactionStatus transactionStatus =
            await depositBridge.GetTransactionStatusAsync(depositTrRs.TransactionId!, token);
        OrderStatus newOrderStatus = transactionStatus == BridgeTransactionStatus.Succeed
            ? OrderStatus.Completed
            : OrderStatus.Canceled;

        await UpdateOrderStatusAsync(order.Id, newOrderStatus, token);

        return Result<CheckBalanceResponse>.Success(new CheckBalanceResponse(order.Id, order.FromNetwork,
            order.FromToken, order.Amount, order.Amount, "completed", "Средства успешно переведены."));
    }


    private async Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status, CancellationToken token)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId, token);
        if (order is not null)
        {
            order.OrderStatus = status;
            await dbContext.SaveChangesAsync(token);
        }
    }
}