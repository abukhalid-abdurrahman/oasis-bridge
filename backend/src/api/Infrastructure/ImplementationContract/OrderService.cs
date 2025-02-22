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

        Guid userId = accessor.GetId() ?? throw new ArgumentNullException();

        bool userExists = await dbContext.Users.AnyAsync(x => x.Id == accessor.GetId(), token);
        bool fromNetworkExists = await dbContext.Networks.AnyAsync(x => x.Name == request.FromNetwork, token);
        bool toNetworkExists = await dbContext.Networks.AnyAsync(x => x.Name == request.ToNetwork, token);
        bool fromTokenExists = await dbContext.NetworkTokens.AnyAsync(x => x.Symbol == request.FromToken, token);
        bool toTokenExists = await dbContext.NetworkTokens.AnyAsync(x => x.Symbol == request.ToToken, token);

        if (!(userExists && fromNetworkExists && toNetworkExists && fromTokenExists && toTokenExists))
            return Result<CreateOrderResponse>.Failure(ResultPatternError.NotFound("Invalid input data"));

        VirtualAccount? virtualAccount = await dbContext.VirtualAccounts
            .Include(x => x.Network)
            .FirstOrDefaultAsync(x =>
                x.UserId == userId && x.Network.Name == request.FromNetwork, token);

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
                UserId = userId,
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

            Result<decimal> balance = await withdrawBridge.GetAccountBalanceAsync(virtualAccount.Address, token);
            if (!balance.IsSuccess)
                return Result<CreateOrderResponse>.Failure(
                    ResultPatternError.InternalServerError("Error getting account balance"));
            bool isTransactional = balance.Value >= request.Amount;

            Order newOrder = new Order
            {
                UserId = userId,
                CreatedBy = accessor.GetId(),
                CreatedByIp = accessor.GetRemoteIpAddress(),
                FromToken = request.FromToken,
                ToToken = request.ToToken,
                FromNetwork = request.FromNetwork,
                ToNetwork = request.ToNetwork,
                DestinationAddress = request.DestinationAddress,
                ExchangeRateId = exchangeRate.Id,
                Amount = request.Amount,
            };

            if (isTransactional)
            {
                Result<TransactionResponse> withdrawTrRs =
                    await withdrawBridge.WithdrawAsync(request.Amount, virtualAccount.Address,
                        virtualAccount.PrivateKey);
                if (!withdrawTrRs.IsSuccess)
                    return Result<CreateOrderResponse>.Failure(
                        ResultPatternError.InternalServerError("Error sending transaction"));

                Result<TransactionResponse> depositTrRs =
                    await depositBridge.DepositAsync(convertedAmount, request.DestinationAddress);
                if (!depositTrRs.IsSuccess)
                {
                    Result<TransactionResponse> abortTrRs =
                        await withdrawBridge.DepositAsync(request.Amount, virtualAccount.Address);
                    if (!abortTrRs.IsSuccess)
                        return Result<CreateOrderResponse>.Failure(
                            ResultPatternError.InternalServerError("Transaction failed, rollback attempted"));
                }

                Result<BridgeTransactionStatus> transactionStatus =
                    await depositBridge.GetTransactionStatusAsync(depositTrRs.Value?.TransactionId!, token);
                if (!transactionStatus.IsSuccess)
                    ResultPatternError.InternalServerError("Error in get transaction status");

                OrderStatus orderStatus = transactionStatus.Value switch
                {
                    BridgeTransactionStatus.Canceled => OrderStatus.Canceled,
                    BridgeTransactionStatus.Completed => OrderStatus.Completed,
                    BridgeTransactionStatus.Pending => OrderStatus.Pending,
                    BridgeTransactionStatus.Expired => OrderStatus.Expired,
                    BridgeTransactionStatus.InsufficientFunds => OrderStatus.InsufficientFunds,
                    BridgeTransactionStatus.SufficientFunds => OrderStatus.SufficientFunds,
                    BridgeTransactionStatus.InsufficientFundsForFee => OrderStatus.InsufficientFundsForFee,
                    _ => OrderStatus.NotFound
                };

                newOrder.OrderStatus = orderStatus;
                newOrder.TransactionHash = depositTrRs.Value?.TransactionId!;
            }

            await dbContext.Orders.AddAsync(newOrder, token);
            int res = await dbContext.SaveChangesAsync(token);

            return res != 0
                ? Result<CreateOrderResponse>.Success(new CreateOrderResponse(newOrder.Id, "Order created"))
                : Result<CreateOrderResponse>.Failure(ResultPatternError.InternalServerError("Could not create order"));
        }

        return Result<CreateOrderResponse>.Failure(ResultPatternError.BadRequest("Unsupported token pair"));
    }


    private async Task<VirtualAccount?> CreateVirtualAccountAsync(CreateOrderRequest request, CancellationToken token)
    {
        VirtualAccount newVirtualAccount = new()
        {
            UserId = accessor.GetId() ?? throw new ArgumentNullException(),
            CreatedBy = HttpAccessor.SystemId,
            CreatedByIp = accessor.GetRemoteIpAddress()
        };

        if (request.FromNetwork == Solana)
        {
            Result<(string PublicKey, string PrivateKey, string SeedPhrase)> response =
                await solanaBridge.CreateAccountAsync(token);
            if (!response.IsSuccess) throw new Exception("Could not create account");
            await PopulateSolanaAccountAsync(newVirtualAccount, response.Value.PublicKey, response.Value.PrivateKey,
                response.Value.SeedPhrase, token);
        }
        else if (request.FromNetwork == Radix)
        {
            Result<(PublicKey PublicKey, PrivateKey PrivateKey, string SeedPhrase)> response =
                radixBridge.CreateAccountAsync(token);
            if (!response.IsSuccess) throw new Exception("Could not create account");
            await PopulateRadixAccountAsync(newVirtualAccount, response.Value.PublicKey, response.Value.PrivateKey,
                response.Value.SeedPhrase, token);
        }

        return newVirtualAccount;
    }

    private async Task PopulateSolanaAccountAsync(VirtualAccount virtualAccount, string publicKey,
        string privateKey,
        string seedPhrase, CancellationToken token)
    {
        virtualAccount.NetworkId = await dbContext.Networks
            .Where(x => x.Name == Solana)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(token);
        virtualAccount.PublicKey = publicKey;
        virtualAccount.PrivateKey = privateKey;
        virtualAccount.SeedPhrase = seedPhrase;
        virtualAccount.Address = publicKey;
        virtualAccount.NetworkType = NetworkType.Solana;
    }

    private async Task PopulateRadixAccountAsync(
        VirtualAccount virtualAccount,
        PublicKey publicKey,
        PrivateKey privateKey,
        string seedPhrase,
        CancellationToken token)
    {
        virtualAccount.NetworkId = await dbContext.Networks
            .Where(x => x.Name == Radix)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(token);
        virtualAccount.PublicKey = Encoders.Hex.EncodeData(privateKey.PublicKeyBytes());
        virtualAccount.PrivateKey = privateKey.RawHex();
        virtualAccount.SeedPhrase = seedPhrase;
        virtualAccount.Address = radixBridge.GetAddressAsync(publicKey,
                                     AddressType.Account, radixOp.NetworkId == 0x01
                                         ? RadixBridge.Enums.NetworkType.Main
                                         : RadixBridge.Enums.NetworkType.Test, token).Value ??
                                 throw new Exception("Invalid to get address account");
        virtualAccount.NetworkType = NetworkType.Radix;
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
            .Include(x => x.Network)
            .FirstOrDefaultAsync(x => x.UserId == order.UserId && x.Network.Name == order.FromNetwork, token);
        if (virtualAccount is null)
            return Result<CheckBalanceResponse>.Failure(ResultPatternError.NotFound("Virtual account not found"));

        Result<decimal> balance = await bridge.GetAccountBalanceAsync(virtualAccount.Address, token);

        if (!balance.IsSuccess)
            return Result<CheckBalanceResponse>.Failure(
                ResultPatternError.InternalServerError("Error in get account balance"));

        if (order.OrderStatus == OrderStatus.Completed)
            return Result<CheckBalanceResponse>.Success(new CheckBalanceResponse(
                order.Id,
                order.FromNetwork,
                order.FromToken,
                balance.Value,
                order.Amount,
                order.Status.ToString(),
                "Order is already completed.",
                order.TransactionHash));

        bool isExpired = (DateTimeOffset.UtcNow - order.CreatedAt).TotalMinutes <= 10;
        if (!isExpired)
        {
            int res = await UpdateOrderStatusAsync(order.Id, OrderStatus.Canceled, token);
            return res != 0
                ? Result<CheckBalanceResponse>.Failure(
                    ResultPatternError.BadRequest(
                        "The replenishment timeout has expired. The order has been rejected."))
                : Result<CheckBalanceResponse>.Failure(ResultPatternError.InternalServerError());
        }

        if (balance.Value < order.Amount
            && order.OrderStatus != OrderStatus.Completed
            && order.OrderStatus != OrderStatus.Canceled)
        {
            return Result<CheckBalanceResponse>.Success(new CheckBalanceResponse(
                order.Id,
                order.FromNetwork,
                order.FromToken,
                balance.Value,
                order.Amount,
                OrderStatus.InsufficientFunds.ToString(),
                "Insufficient funds. Waiting for replenishment.",
                order.TransactionHash
            ));
        }

        return await ProcessTransactionAsync(order, virtualAccount, token);
    }


    private async Task<Result<CheckBalanceResponse>> ProcessTransactionAsync(Order order, VirtualAccount virtualAccount,
        CancellationToken token)
    {
        if (order.OrderStatus != OrderStatus.Canceled && order.OrderStatus != OrderStatus.Completed)
        {
            IBridge withdrawBridge = order.FromToken == Xrd ? radixBridge : solanaBridge;
            IBridge depositBridge = order.FromToken == Xrd ? solanaBridge : radixBridge;

            Result<TransactionResponse> withdrawTrRs =
                await withdrawBridge.WithdrawAsync(order.Amount, virtualAccount.Address, virtualAccount.PrivateKey);
            if (!withdrawTrRs.IsSuccess)
                return Result<CheckBalanceResponse>.Failure(
                    ResultPatternError.InternalServerError("Error in withdraw"));

            Result<TransactionResponse> depositTrRs =
                await depositBridge.DepositAsync(order.Amount, order.DestinationAddress);
            if (!depositTrRs.IsSuccess)
            {
                await withdrawBridge.DepositAsync(order.Amount, virtualAccount.Address);
                return Result<CheckBalanceResponse>.Failure(
                    ResultPatternError.InternalServerError("Error in deposit"));
            }

            Result<BridgeTransactionStatus> transactionStatus =
                await depositBridge.GetTransactionStatusAsync(depositTrRs.Value?.TransactionId!, token);
            if (!transactionStatus.IsSuccess)
                ResultPatternError.InternalServerError("Error in get transaction status");

            OrderStatus orderStatus = transactionStatus.Value switch
            {
                BridgeTransactionStatus.Canceled => OrderStatus.Canceled,
                BridgeTransactionStatus.Completed => OrderStatus.Completed,
                BridgeTransactionStatus.Pending => OrderStatus.Pending,
                BridgeTransactionStatus.Expired => OrderStatus.Expired,
                BridgeTransactionStatus.InsufficientFunds => OrderStatus.InsufficientFunds,
                BridgeTransactionStatus.SufficientFunds => OrderStatus.SufficientFunds,
                BridgeTransactionStatus.InsufficientFundsForFee => OrderStatus.InsufficientFundsForFee,
                _ => OrderStatus.NotFound
            };

            order.Status = order.Status;
            order.TransactionHash = depositTrRs.Value?.TransactionId;
            dbContext.Orders.Update(order);
            int res = await dbContext.SaveChangesAsync(token);

            return res != 0
                ? Result<CheckBalanceResponse>.Success(
                    new CheckBalanceResponse(
                        order.Id,
                        order.FromNetwork,
                        order.FromToken,
                        order.Amount,
                        order.Amount,
                        OrderStatus.Completed.ToString(),
                        "Success",
                        order.TransactionHash))
                : Result<CheckBalanceResponse>.Failure(ResultPatternError.InternalServerError());
        }

        return Result<CheckBalanceResponse>.Success(new CheckBalanceResponse(
            order.Id,
            order.FromNetwork,
            order.FromToken,
            order.Amount,
            order.Amount,
            OrderStatus.Completed.ToString(),
            "Success",
            order.TransactionHash));
    }

    private async Task<int> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, CancellationToken token)
    {
        Order? order = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId, token);
        if (order is not null)
        {
            if (order.OrderStatus == status) return 1;
            order.OrderStatus = status;
            return await dbContext.SaveChangesAsync(token);
        }

        return 0;
    }
}