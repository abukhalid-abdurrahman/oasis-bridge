using NetworkType = RadixBridge.Enums.NetworkType;

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

        Guid? userId = accessor.GetId();
        if (userId is null)
            return Result<CreateOrderResponse>.Failure(ResultPatternError.BadRequest("UserId cannot be null."));

        Result<CreateOrderResponse> validationRequest = await ValidateRequestAsync(userId, request, token);
        if (!validationRequest.IsSuccess)
            return validationRequest;

        IBridge withdrawBridge = request.FromToken == Xrd ? radixBridge : solanaBridge;
        IBridge depositBridge = request.FromToken == Xrd ? solanaBridge : radixBridge;


        ExchangeRate exchangeRate = await dbContext.ExchangeRates.AsNoTracking()
            .Include(x => x.FromToken)
            .Include(x => x.ToToken)
            .Where(x => x.FromToken.Symbol == request.FromToken
                        && x.ToToken.Symbol == request.ToToken)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(token) ?? new();
        decimal convertedAmount = exchangeRate.Rate * request.Amount;

        VirtualAccount? virtualAccount = await dbContext.VirtualAccounts
            .AsNoTracking()
            .Include(x => x.Network)
            .FirstOrDefaultAsync(x =>
                x.UserId == userId && x.Network.Name == request.FromNetwork, token);


        if (virtualAccount is null)
        {
            Result<(string publicKey, string privateKey, string seedPhrase)> resultCreateAccount =
                await withdrawBridge.CreateAccountAsync(token);
            if (!resultCreateAccount.IsSuccess)
                return Result<CreateOrderResponse>.Failure(resultCreateAccount.Error);

            Result<string> resultGetAddress = radixBridge.GetAddressAsync(
                new PrivateKey(Encoders.Hex.DecodeData(resultCreateAccount.Value.privateKey), Curve.ED25519)
                    .PublicKey(),
                AddressType.Account,
                radixOp.NetworkId == 0x01 ? NetworkType.Main : NetworkType.Test);
            if (!resultGetAddress.IsSuccess)
                return Result<CreateOrderResponse>.Failure(resultGetAddress.Error);

            VirtualAccount newVirtualAccount = new()
            {
                PublicKey = resultCreateAccount.Value.publicKey,
                PrivateKey = resultCreateAccount.Value.privateKey,
                SeedPhrase = resultCreateAccount.Value.seedPhrase,
                Address = request.FromToken == Xrd
                    ? resultGetAddress.Value!
                    : resultCreateAccount.Value.publicKey,
                CreatedBy = accessor.GetId(),
                CreatedByIp = accessor.GetRemoteIpAddress(),
                UserId = (Guid)userId,
                NetworkId = await dbContext.Networks
                    .Where(x => x.Name == request.FromNetwork)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(token),
                NetworkType = request.FromToken == Xrd
                    ? Domain.Enums.NetworkType.Radix
                    : Domain.Enums.NetworkType.Solana
            };
            await dbContext.VirtualAccounts.AddAsync(newVirtualAccount, token);
            int result = await dbContext.SaveChangesAsync(token);
            if (result == 0)
                return Result<CreateOrderResponse>.Failure(
                    ResultPatternError.InternalServerError("Couldn't create virtual account'"));

            Order newOrder = new()
            {
                Amount = request.Amount,
                OrderStatus = OrderStatus.InsufficientFunds,
                UserId = (Guid)userId,
                ExchangeRateId = exchangeRate.Id,
                CreatedByIp = accessor.GetRemoteIpAddress(),
                CreatedBy = accessor.GetId(),
                FromToken = request.FromToken,
                ToToken = request.ToToken,
                FromNetwork = request.FromNetwork,
                ToNetwork = request.ToNetwork,
                DestinationAddress = request.DestinationAddress,
            };
            await dbContext.Orders.AddAsync(newOrder, token);
            int response = await dbContext.SaveChangesAsync(token);

            return response != 0
                ? Result<CreateOrderResponse>.Success(
                    new CreateOrderResponse(newOrder.Id, "Successfully order created"))
                : Result<CreateOrderResponse>.Failure(
                    ResultPatternError.InternalServerError("Error in creating order"));
        }

        if (request is { FromToken: Xrd, ToToken: Sol } or { FromToken: Sol, ToToken: Xrd })
        {
            Result<decimal> balance = await withdrawBridge.GetAccountBalanceAsync(virtualAccount.Address, token);
            if (!balance.IsSuccess)
                return Result<CreateOrderResponse>.Failure(balance.Error);

            bool isTransactional = balance.Value > request.Amount;

            Order newOrder = new Order
            {
                UserId = (Guid)userId,
                CreatedBy = accessor.GetId(),
                CreatedByIp = accessor.GetRemoteIpAddress(),
                FromToken = request.FromToken,
                ToToken = request.ToToken,
                FromNetwork = request.FromNetwork,
                ToNetwork = request.ToNetwork,
                DestinationAddress = request.DestinationAddress,
                ExchangeRateId = exchangeRate.Id,
                Amount = request.Amount,
                OrderStatus = OrderStatus.InsufficientFunds,
            };

            if (isTransactional)
            {
                Result<TransactionResponse> withdrawTrRs = default!;
                Result<TransactionResponse> depositTrRs = default!;
                Result<TransactionResponse> abortTrRs;
                try
                {
                    withdrawTrRs = await withdrawBridge.WithdrawAsync(request.Amount, virtualAccount.Address,
                        virtualAccount.PrivateKey);
                    if (!withdrawTrRs.IsSuccess)
                        return Result<CreateOrderResponse>.Failure(withdrawTrRs.Error);

                    depositTrRs =
                        await depositBridge.DepositAsync(convertedAmount, request.DestinationAddress);
                    if (!depositTrRs.IsSuccess)
                    {
                        abortTrRs =
                            await withdrawBridge.DepositAsync(request.Amount, virtualAccount.Address);
                        if (!abortTrRs.IsSuccess)
                            return Result<CreateOrderResponse>.Failure(abortTrRs.Error);

                        return Result<CreateOrderResponse>.Failure(depositTrRs.Error);
                    }
                }
                catch (Exception e)
                {
                    if (withdrawTrRs.IsSuccess && (depositTrRs is null || !depositTrRs.IsSuccess))
                    {
                        abortTrRs =
                            await withdrawBridge.DepositAsync(request.Amount, virtualAccount.Address);
                        if (!abortTrRs.IsSuccess)
                            return Result<CreateOrderResponse>.Failure(abortTrRs.Error);
                    }

                    return Result<CreateOrderResponse>.Failure(ResultPatternError.InternalServerError(e.Message));
                }

                Result<BridgeTransactionStatus> transactionStatus =
                    await depositBridge.GetTransactionStatusAsync(depositTrRs.Value?.TransactionId!, token);
                if (!transactionStatus.IsSuccess)
                    return Result<CreateOrderResponse>.Failure(transactionStatus.Error);

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
                newOrder.TransactionHash = depositTrRs.Value?.TransactionId;
            }


            await dbContext.Orders.AddAsync(newOrder, token);
            int res = await dbContext.SaveChangesAsync(token);

            return res != 0
                ? Result<CreateOrderResponse>.Success(new CreateOrderResponse(newOrder.Id, "Order created"))
                : Result<CreateOrderResponse>.Failure(ResultPatternError.InternalServerError("Could not create order"));
        }

        return Result<CreateOrderResponse>.Failure(ResultPatternError.BadRequest("Unsupported token pair"));
    }


    private async Task<Result<CreateOrderResponse>> ValidateRequestAsync(Guid? userId, CreateOrderRequest request,
        CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();

            if (!await dbContext.Users.AnyAsync(x => x.Id == userId, token))
                return Result<CreateOrderResponse>.Failure(
                    ResultPatternError.NotFound($"User with Id: {userId} not found"));

            if (!await dbContext.Networks.AnyAsync(x => x.Name == request.FromNetwork, token))
                return Result<CreateOrderResponse>.Failure(
                    ResultPatternError.NotFound($"Network with Name: {request.FromNetwork} not found"));

            if (!await dbContext.Networks.AnyAsync(x => x.Name == request.ToNetwork, token))
                return Result<CreateOrderResponse>.Failure(
                    ResultPatternError.NotFound($"Network with Name: {request.ToNetwork} not found"));

            if (!await dbContext.NetworkTokens.AnyAsync(x => x.Symbol == request.FromToken, token))
                return Result<CreateOrderResponse>.Failure(
                    ResultPatternError.NotFound($"Token with Symbol: {request.FromToken} not found"));

            if (!await dbContext.NetworkTokens.AnyAsync(x => x.Symbol == request.ToToken, token))
                return Result<CreateOrderResponse>.Failure(
                    ResultPatternError.NotFound($"Token with Symbol: {request.ToToken} not found"));

            if (request.Amount <= 0)
                return Result<CreateOrderResponse>.Failure(
                    ResultPatternError.BadRequest("Amount must be greater than zero."));
            return Result<CreateOrderResponse>.Success();
        }
        catch (Exception e)
        {
            return Result<CreateOrderResponse>.Failure(ResultPatternError.InternalServerError(e.Message));
        }
    }


    public async Task<Result<CheckBalanceResponse>> CheckBalanceAsync(Guid orderId, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        Order? order = await dbContext.Orders
            .FirstOrDefaultAsync(x => x.Id == orderId, token);

        if (order is null)
            return Result<CheckBalanceResponse>
                .Failure(ResultPatternError.NotFound($"Order with Id: {orderId} not found"));

        IBridge withdrawBridge = order.FromToken == Xrd ? radixBridge : solanaBridge;
        IBridge depositBridge = order.FromToken == Xrd ? solanaBridge : radixBridge;


        VirtualAccount? virtualAccount = await dbContext.VirtualAccounts
            .AsNoTracking()
            .Include(x => x.Network)
            .FirstOrDefaultAsync(x => x.UserId == order.UserId && x.Network.Name == order.FromNetwork, token);
        if (virtualAccount is null)
            return Result<CheckBalanceResponse>.Failure(ResultPatternError.NotFound("Virtual account not found"));

        ExchangeRate exchangeRate =
            await dbContext.ExchangeRates.FirstOrDefaultAsync(
                x => x.Id == order.ExchangeRateId, token) ?? new();

        decimal convertedAmount = exchangeRate.Rate * order.Amount;


        Result<decimal> balance = await withdrawBridge.GetAccountBalanceAsync(virtualAccount.Address, token);

        if (balance is { IsSuccess: false })
            return Result<CheckBalanceResponse>.Failure(balance.Error);

        if (order.OrderStatus == OrderStatus.Completed)
            return Result<CheckBalanceResponse>.Success(new CheckBalanceResponse(
                order.Id,
                order.FromNetwork,
                order.FromToken,
                balance.Value,
                0,
                order.OrderStatus.ToString(),
                "Order is already completed.",
                order.TransactionHash));

        bool isExpired = (DateTimeOffset.UtcNow - order.CreatedAt).TotalMinutes > 10;
        if (isExpired)
        {
            order.OrderStatus = OrderStatus.Canceled;
            int res = await dbContext.SaveChangesAsync(token);
            return res != 0
                ? Result<CheckBalanceResponse>.Success(
                    new(order.Id,
                        order.FromNetwork,
                        order.FromToken,
                        balance.Value,
                        order.Amount,
                        OrderStatus.Canceled.ToString(),
                        "Order is already completed.",
                        order.TransactionHash))
                : Result<CheckBalanceResponse>.Failure(ResultPatternError.InternalServerError());
        }

        if (balance.Value <= order.Amount
            && order.OrderStatus != OrderStatus.Completed
            && order.OrderStatus != OrderStatus.Canceled)
        {
            return Result<CheckBalanceResponse>.Success(
                new CheckBalanceResponse(
                    order.Id,
                    order.FromNetwork,
                    order.FromToken,
                    balance.Value,
                    order.Amount,
                    order.OrderStatus.ToString(),
                    "Insufficient funds. Waiting for replenishment or Insufficient funds to cover transaction fees.",
                    order.TransactionHash
                ));
        }
        else if (balance.Value > order.Amount && order.OrderStatus != OrderStatus.Canceled &&
                 order.OrderStatus != OrderStatus.Completed)
        {
            Result<TransactionResponse> withdrawTrRs = default!;
            Result<TransactionResponse> depositTrRs = default!;
            Result<TransactionResponse> abortTrRs;
            try
            {
                withdrawTrRs = await withdrawBridge.WithdrawAsync(order.Amount, virtualAccount.Address,
                    virtualAccount.PrivateKey);
                if (!withdrawTrRs.IsSuccess)
                    return Result<CheckBalanceResponse>.Failure(withdrawTrRs.Error);

                depositTrRs =
                    await depositBridge.DepositAsync(convertedAmount, order.DestinationAddress);
                if (!depositTrRs.IsSuccess)
                {
                    abortTrRs =
                        await withdrawBridge.DepositAsync(order.Amount, virtualAccount.Address);
                    if (!abortTrRs.IsSuccess)
                        return Result<CheckBalanceResponse>.Failure(abortTrRs.Error);

                    return Result<CheckBalanceResponse>.Failure(depositTrRs.Error);
                }
            }
            catch (Exception e)
            {
                if (withdrawTrRs.IsSuccess && (depositTrRs is null || !depositTrRs.IsSuccess))
                {
                    abortTrRs =
                        await withdrawBridge.DepositAsync(order.Amount, virtualAccount.Address);
                    if (!abortTrRs.IsSuccess)
                        return Result<CheckBalanceResponse>.Failure(abortTrRs.Error);
                }

                return Result<CheckBalanceResponse>.Failure(ResultPatternError.InternalServerError(e.Message));
            }

            Result<BridgeTransactionStatus> transactionStatus =
                await depositBridge.GetTransactionStatusAsync(depositTrRs.Value?.TransactionId!, token);
            if (!transactionStatus.IsSuccess)
                return Result<CheckBalanceResponse>.Failure(transactionStatus.Error);

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

            order.OrderStatus = orderStatus;
            order.TransactionHash = depositTrRs.Value?.TransactionId;
            
            int res = await dbContext.SaveChangesAsync(token);
            
            balance = await withdrawBridge.GetAccountBalanceAsync(virtualAccount.Address, token);
            return res != 0
                ? Result<CheckBalanceResponse>.Success(
                    new CheckBalanceResponse(
                        order.Id,
                        order.FromNetwork,
                        order.FromToken,
                        balance.Value,
                        0,
                        OrderStatus.Completed.ToString(),
                        "Success",
                        order.TransactionHash))
                : Result<CheckBalanceResponse>.Failure(ResultPatternError.InternalServerError());
        }

        return Result<CheckBalanceResponse>.Success(new CheckBalanceResponse(
            order.Id,
            order.FromNetwork,
            order.FromToken,
            balance.Value,
            0,
            order.OrderStatus.ToString(),
            "",
            order.TransactionHash));
    }
}