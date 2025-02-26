using NetworkType = RadixBridge.Enums.NetworkType;

namespace Infrastructure.ImplementationContract;

public sealed class OrderService(
    DataContext dbContext,
    IHttpContextAccessor accessor,
    ISolanaBridge solanaBridge,
    IRadixBridge radixBridge,
    RadixTechnicalAccountBridgeOptions radixOp,
    ILogger<OrderService> logger) : IOrderService
{
    private const string Sol = "SOL";
    private const string Xrd = "XRD";

    public async Task<Result<CreateOrderResponse>> CreateOrderAsync(CreateOrderRequest request,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        logger.LogInformation("Starting CreateOrderAsync at {Time}", DateTimeOffset.UtcNow);

        // Step 1: Retrieve user ID from token
        logger.LogInformation("Retrieving user ID from token...");
        Guid? userId = accessor.GetId();
        if (userId is null)
        {
            logger.LogWarning("User ID is null.");
            return Result<CreateOrderResponse>.Failure(ResultPatternError.BadRequest("UserId cannot be null."));
        }

        // Step 2: Validate the incoming request
        logger.LogInformation("Validating request...");
        Result<CreateOrderResponse> validationRequest = await ValidateRequestAsync(userId, request, token);
        if (!validationRequest.IsSuccess)
        {
            logger.LogWarning("Request validation failed: {Error}", validationRequest.Error.Message);
            return validationRequest;
        }

        // Step 3: Define bridges for withdrawal and deposit based on token pair
        logger.LogInformation("Determining which bridge to use for withdrawal and deposit...");
        IBridge withdrawBridge = request.FromToken == Xrd ? radixBridge : solanaBridge;
        IBridge depositBridge = request.FromToken == Xrd ? solanaBridge : radixBridge;

        // Step 4: Retrieve the latest exchange rate for the token pair
        logger.LogInformation("Retrieving exchange rate for {FromToken} to {ToToken}...", request.FromToken,
            request.ToToken);
        ExchangeRate exchangeRate = await dbContext.ExchangeRates.AsNoTracking()
            .Include(x => x.FromToken)
            .Include(x => x.ToToken)
            .Where(x => x.FromToken.Symbol == request.FromToken && x.ToToken.Symbol == request.ToToken)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(token) ?? new();
        decimal convertedAmount = exchangeRate.Rate * request.Amount;
        logger.LogInformation("Exchange rate retrieved. Rate: {Rate}, Converted amount: {ConvertedAmount}",
            exchangeRate.Rate, convertedAmount);

        // Step 5: Retrieve virtual account for the user in the given network
        logger.LogInformation("Checking if virtual account exists for user {UserId} on network {Network}...",
            userId, request.FromNetwork);
        VirtualAccount? virtualAccount = await dbContext.VirtualAccounts
            .AsNoTracking()
            .Include(x => x.Network)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Network.Name == request.FromNetwork, token);

        // Step 6: If no virtual account exists, create one
        if (virtualAccount is null)
        {
            logger.LogInformation("Virtual account not found. Creating new virtual account...");
            Result<(string publicKey, string privateKey, string seedPhrase)> resultCreateAccount =
                await withdrawBridge.CreateAccountAsync(token);
            if (!resultCreateAccount.IsSuccess)
            {
                logger.LogError("Failed to create virtual account: {Error}", resultCreateAccount.Error.Message);
                return Result<CreateOrderResponse>.Failure(resultCreateAccount.Error);
            }

            logger.LogInformation("Virtual account created. Determining account address...");
            string address = resultCreateAccount.Value.publicKey;
            if (request.FromToken == Xrd)
            {
                logger.LogInformation("Retrieving address for Radix account...");
                Result<string> resultGetAddress = radixBridge.GetAddressAsync(
                    new PrivateKey(Encoders.Hex.DecodeData(resultCreateAccount.Value.privateKey), Curve.ED25519)
                        .PublicKey(),
                    AddressType.Account,
                    radixOp.NetworkId == 0x01 ? NetworkType.Main : NetworkType.Test);
                if (!resultGetAddress.IsSuccess)
                {
                    logger.LogError("Failed to retrieve account address: {Error}", resultGetAddress.Error.Message);
                    return Result<CreateOrderResponse>.Failure(resultGetAddress.Error);
                }

                address = resultGetAddress.Value!;
                logger.LogInformation("Retrieved Radix account address: {Address}", address);
            }

            logger.LogInformation("Creating new VirtualAccount object...");
            VirtualAccount newVirtualAccount = new()
            {
                PublicKey = resultCreateAccount.Value.publicKey,
                PrivateKey = resultCreateAccount.Value.privateKey,
                SeedPhrase = resultCreateAccount.Value.seedPhrase,
                Address = address,
                CreatedBy = accessor.GetId(),
                CreatedByIp = accessor.GetRemoteIpAddress(),
                UserId = (Guid)userId,
                NetworkId = await dbContext.Networks.Where(x => x.Name == request.FromNetwork).Select(x => x.Id)
                    .FirstOrDefaultAsync(token),
                NetworkType = request.FromToken == Xrd
                    ? Domain.Enums.NetworkType.Radix
                    : Domain.Enums.NetworkType.Solana
            };

            logger.LogInformation("Adding new virtual account to the database...");
            await dbContext.VirtualAccounts.AddAsync(newVirtualAccount, token);
            int createResult = await dbContext.SaveChangesAsync(token);
            if (createResult == 0)
            {
                logger.LogError("Failed to save new virtual account to the database.");
                return Result<CreateOrderResponse>.Failure(
                    ResultPatternError.InternalServerError("Couldn't create virtual account."));
            }

            // Create a new order linked to this new virtual account
            logger.LogInformation("Creating new order for new virtual account...");
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

            logger.LogInformation("Adding new order to the database...");
            await dbContext.Orders.AddAsync(newOrder, token);
            int orderResult = await dbContext.SaveChangesAsync(token);
            logger.LogInformation("Order saved in database? {Result}", orderResult != 0);
            return orderResult != 0
                ? Result<CreateOrderResponse>.Success(new CreateOrderResponse(newOrder.Id,
                    "Successfully order created"))
                : Result<CreateOrderResponse>.Failure(
                    ResultPatternError.InternalServerError("Error in creating order"));
        }

        // Step 7: If virtual account exists, process the order based on token pair
        logger.LogInformation("Virtual account exists. Processing order for existing account.");
        if (request is { FromToken: Xrd, ToToken: Sol } or { FromToken: Sol, ToToken: Xrd })
        {
            logger.LogInformation("Retrieving balance for virtual account {Address}", virtualAccount.Address);
            Result<decimal> balance = await withdrawBridge.GetAccountBalanceAsync(virtualAccount.Address, token);
            if (!balance.IsSuccess)
            {
                logger.LogError("Failed to retrieve balance: {Error}", balance.Error.Message);
                return Result<CreateOrderResponse>.Failure(balance.Error);
            }

            logger.LogInformation("Balance retrieved: {Balance} SOL", balance.Value);

            logger.LogInformation("Checking if balance is sufficient for transaction...");
            bool isTransactional = balance.Value > request.Amount;

            logger.LogInformation("Creating new Order object for existing virtual account.");
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

            // If balance is sufficient, proceed with executing withdrawal and deposit transactions
            if (isTransactional)
            {
                logger.LogInformation("Balance is sufficient. Initiating transaction execution.");
                Result<TransactionResponse> withdrawTrRs = default!;
                Result<TransactionResponse> depositTrRs = default!;
                Result<TransactionResponse> abortTrRs;
                try
                {
                    logger.LogInformation("Executing withdrawal transaction for amount {Amount}", request.Amount);
                    withdrawTrRs = await withdrawBridge.WithdrawAsync(request.Amount, virtualAccount.Address,
                        virtualAccount.PrivateKey);
                    if (!withdrawTrRs.IsSuccess)
                    {
                        logger.LogError("Withdrawal transaction failed: {Error}", withdrawTrRs.Error.Message);
                        return Result<CreateOrderResponse>.Failure(withdrawTrRs.Error);
                    }

                    logger.LogInformation("Executing deposit transaction with converted amount {ConvertedAmount}",
                        convertedAmount);
                    depositTrRs = await depositBridge.DepositAsync(convertedAmount, request.DestinationAddress);
                    if (!depositTrRs.IsSuccess)
                    {
                        logger.LogWarning("Deposit transaction failed, attempting to abort withdrawal...");
                        abortTrRs = await withdrawBridge.DepositAsync(request.Amount, virtualAccount.Address);
                        if (!abortTrRs.IsSuccess)
                        {
                            logger.LogError("Abort withdrawal failed: {Error}", abortTrRs.Error.Message);
                            return Result<CreateOrderResponse>.Failure(abortTrRs.Error);
                        }

                        logger.LogWarning("Returning deposit transaction error: {Error}",
                            depositTrRs.Error.Message);
                        return Result<CreateOrderResponse>.Failure(depositTrRs.Error);
                    }

                    logger.LogInformation("Retrieving transaction status for deposit transaction...");
                    Result<BridgeTransactionStatus> transactionStatus =
                        await depositBridge.GetTransactionStatusAsync(depositTrRs.Value?.TransactionId!, token);
                    if (!transactionStatus.IsSuccess)
                    {
                        logger.LogError("Failed to retrieve transaction status: {Error}",
                            transactionStatus.Error.Message);
                        return Result<CreateOrderResponse>.Failure(transactionStatus.Error);
                    }

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
                    logger.LogInformation("Transaction status interpreted as: {Status}", orderStatus);

                    newOrder.OrderStatus = orderStatus;
                    newOrder.TransactionHash = depositTrRs.Value?.TransactionId;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Exception during transaction execution.");
                    if (withdrawTrRs.IsSuccess && (depositTrRs is null || !depositTrRs.IsSuccess))
                    {
                        logger.LogInformation(
                            "Attempting to abort withdrawal due to failure in deposit transaction.");
                        abortTrRs = await withdrawBridge.DepositAsync(request.Amount, virtualAccount.Address);
                        if (!abortTrRs.IsSuccess)
                        {
                            logger.LogError("Abort withdrawal failed: {Error}", abortTrRs.Error.Message);
                            return Result<CreateOrderResponse>.Failure(abortTrRs.Error);
                        }
                    }

                    logger.LogError("Returning error due to exception: {Error}", e.Message);
                    return Result<CreateOrderResponse>.Failure(ResultPatternError.InternalServerError(e.Message));
                }
            }

            logger.LogInformation("Saving new order to the database for existing virtual account.");
            await dbContext.Orders.AddAsync(newOrder, token);
            int res = await dbContext.SaveChangesAsync(token);
            logger.LogInformation("Database save result: {Result}", res);
            return res != 0
                ? Result<CreateOrderResponse>.Success(new CreateOrderResponse(newOrder.Id, "Order created"))
                : Result<CreateOrderResponse>.Failure(
                    ResultPatternError.InternalServerError("Could not create order"));
        }

        logger.LogInformation("Unsupported token pair. Exiting CreateOrderAsync.");
        return Result<CreateOrderResponse>.Failure(ResultPatternError.BadRequest("Unsupported token pair"));
    }

    // Validates the incoming CreateOrderRequest
    private async Task<Result<CreateOrderResponse>> ValidateRequestAsync(Guid? userId, CreateOrderRequest request,
        CancellationToken token)
    {
        try
        {
            logger.LogInformation("Starting ValidateRequestAsync at {Time}", DateTimeOffset.UtcNow);
            token.ThrowIfCancellationRequested();

            if (!await dbContext.Users.AnyAsync(x => x.Id == userId, token))
            {
                logger.LogWarning("User with ID {UserId} not found.", userId);
                return Result<CreateOrderResponse>.Failure(
                    ResultPatternError.NotFound($"User with Id: {userId} not found"));
            }

            if (!await dbContext.Networks.AnyAsync(x => x.Name == request.FromNetwork, token))
            {
                logger.LogWarning("Network with Name {Network} not found.", request.FromNetwork);
                return Result<CreateOrderResponse>.Failure(
                    ResultPatternError.NotFound($"Network with Name: {request.FromNetwork} not found"));
            }

            if (!await dbContext.Networks.AnyAsync(x => x.Name == request.ToNetwork, token))
            {
                logger.LogWarning("Network with Name {Network} not found.", request.ToNetwork);
                return Result<CreateOrderResponse>.Failure(
                    ResultPatternError.NotFound($"Network with Name: {request.ToNetwork} not found"));
            }

            if (!await dbContext.NetworkTokens.AnyAsync(x => x.Symbol == request.FromToken, token))
            {
                logger.LogWarning("Token with Symbol {Token} not found.", request.FromToken);
                return Result<CreateOrderResponse>.Failure(
                    ResultPatternError.NotFound($"Token with Symbol: {request.FromToken} not found"));
            }

            if (!await dbContext.NetworkTokens.AnyAsync(x => x.Symbol == request.ToToken, token))
            {
                logger.LogWarning("Token with Symbol {Token} not found.", request.ToToken);
                return Result<CreateOrderResponse>.Failure(
                    ResultPatternError.NotFound($"Token with Symbol: {request.ToToken} not found"));
            }

            if (request.Amount <= 0)
            {
                logger.LogWarning("Invalid amount: {Amount}. Must be greater than zero.", request.Amount);
                return Result<CreateOrderResponse>.Failure(
                    ResultPatternError.BadRequest("Amount must be greater than zero."));
            }

            // Validate destination address format based on network
            if (request.FromToken == Xrd)
            {
                logger.LogInformation("Validating destination address as Solana address.");
                bool check = IsValidSolanaAddress(request.DestinationAddress);
                if (!check)
                {
                    logger.LogWarning("Invalid Solana address format: {Address}", request.DestinationAddress);
                    return Result<CreateOrderResponse>.Failure(
                        ResultPatternError.BadRequest("Invalid Solana format address"));
                }
            }
            else
            {
                logger.LogInformation("Validating destination address as Radix address.");
                bool check = IsValidRadixAddress(request.DestinationAddress);
                if (!check)
                {
                    logger.LogWarning("Invalid Radix address format: {Address}", request.DestinationAddress);
                    return Result<CreateOrderResponse>.Failure(
                        ResultPatternError.BadRequest("Invalid Radix format address"));
                }
            }

            logger.LogInformation("Validation completed successfully at {Time}", DateTimeOffset.UtcNow);
            return Result<CreateOrderResponse>.Success();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception in ValidateRequestAsync: {Message}", e.Message);
            return Result<CreateOrderResponse>.Failure(ResultPatternError.InternalServerError(e.Message));
        }
    }

    // Checks the balance and processes the order if funds are sufficient
    public async Task<Result<CheckBalanceResponse>> CheckBalanceAsync(Guid orderId,
        CancellationToken token = default)
    {
        logger.LogInformation("Starting CheckBalanceAsync for order {OrderId} at {Time}", orderId,
            DateTimeOffset.UtcNow);
        token.ThrowIfCancellationRequested();

        Order? order = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId, token);
        if (order is null)
        {
            logger.LogWarning("Order with ID {OrderId} not found.", orderId);
            return Result<CheckBalanceResponse>.Failure(
                ResultPatternError.NotFound($"Order with Id: {orderId} not found"));
        }

        logger.LogInformation("Determining withdrawal and deposit bridges based on token pair.");
        IBridge withdrawBridge = order.FromToken == Xrd ? radixBridge : solanaBridge;
        IBridge depositBridge = order.FromToken == Xrd ? solanaBridge : radixBridge;

        logger.LogInformation("Retrieving virtual account for user {UserId} on network {Network}", order.UserId,
            order.FromNetwork);
        VirtualAccount? virtualAccount = await dbContext.VirtualAccounts
            .AsNoTracking()
            .Include(x => x.Network)
            .FirstOrDefaultAsync(x => x.UserId == order.UserId && x.Network.Name == order.FromNetwork, token);
        if (virtualAccount is null)
        {
            logger.LogWarning("Virtual account not found for user {UserId}.", order.UserId);
            return Result<CheckBalanceResponse>.Failure(ResultPatternError.NotFound("Virtual account not found"));
        }

        logger.LogInformation("Retrieving exchange rate for order {OrderId}", order.Id);
        ExchangeRate exchangeRate =
            await dbContext.ExchangeRates.FirstOrDefaultAsync(x => x.Id == order.ExchangeRateId, token) ?? new();
        decimal convertedAmount = exchangeRate.Rate * order.Amount;
        logger.LogInformation("Exchange rate: {Rate}, Converted amount: {ConvertedAmount}", exchangeRate.Rate,
            convertedAmount);

        logger.LogInformation("Retrieving balance for virtual account at address {Address}",
            virtualAccount.Address);
        Result<decimal> balance = await withdrawBridge.GetAccountBalanceAsync(virtualAccount.Address, token);
        if (!balance.IsSuccess)
        {
            logger.LogError("Failed to retrieve balance: {Error}", balance.Error.Message);
            return Result<CheckBalanceResponse>.Failure(balance.Error);
        }

        // If order already completed, simply return the balance information.
        if (order.OrderStatus == OrderStatus.Completed)
        {
            logger.LogInformation("Order {OrderId} already completed.", order.Id);
            return Result<CheckBalanceResponse>.Success(new CheckBalanceResponse(
                order.Id,
                order.FromNetwork,
                order.FromToken,
                balance.Value,
                0,
                order.OrderStatus.ToString(),
                "Order is already completed.",
                order.TransactionHash));
        }

        // Check if the order has expired (more than 10 minutes since creation)
        bool isExpired = (DateTimeOffset.UtcNow - order.CreatedAt).TotalMinutes > 10;
        if (isExpired)
        {
            logger.LogInformation("Order {OrderId} has expired. Cancelling order...", order.Id);
            order.OrderStatus = OrderStatus.Canceled;
            int res = await dbContext.SaveChangesAsync(token);
            return res != 0
                ? Result<CheckBalanceResponse>.Success(new CheckBalanceResponse(
                    order.Id,
                    order.FromNetwork,
                    order.FromToken,
                    balance.Value,
                    order.Amount,
                    OrderStatus.Expired.ToString(),
                    "Order is canceled.",
                    order.TransactionHash))
                : Result<CheckBalanceResponse>.Failure(ResultPatternError.InternalServerError());
        }

        // If balance is insufficient to cover the order amount
        if (balance.Value <= order.Amount && order.OrderStatus != OrderStatus.Completed &&
            order.OrderStatus != OrderStatus.Canceled && order.OrderStatus != OrderStatus.Expired)
        {
            logger.LogInformation("Insufficient funds for order {OrderId}. Balance: {Balance}, Required: {Amount}",
                order.Id, balance.Value, order.Amount);
            return Result<CheckBalanceResponse>.Success(new CheckBalanceResponse(
                order.Id,
                order.FromNetwork,
                order.FromToken,
                balance.Value,
                order.Amount,
                order.OrderStatus.ToString(),
                "Insufficient funds. Waiting for replenishment or insufficient funds to cover transaction fees.",
                order.TransactionHash));
        }
        // Otherwise, if balance is sufficient, process the transaction
        else if (balance.Value > order.Amount && order.OrderStatus != OrderStatus.Canceled &&
                 order.OrderStatus != OrderStatus.Completed && order.OrderStatus != OrderStatus.Expired)
        {
            logger.LogInformation(
                "Sufficient funds detected for order {OrderId}. Initiating transaction processing...", order.Id);

            Result<TransactionResponse> withdrawTrRs = default!;
            Result<TransactionResponse> depositTrRs = default!;
            Result<TransactionResponse> abortTrRs;
            try
            {
                logger.LogInformation("Executing withdrawal transaction for order {OrderId}...", order.Id);
                withdrawTrRs = await withdrawBridge.WithdrawAsync(order.Amount, virtualAccount.Address,
                    virtualAccount.PrivateKey);
                if (!withdrawTrRs.IsSuccess)
                {
                    logger.LogError("Withdrawal transaction failed: {Error}", withdrawTrRs.Error.Message);
                    return Result<CheckBalanceResponse>.Failure(withdrawTrRs.Error);
                }

                logger.LogInformation(
                    "Executing deposit transaction for order {OrderId} with converted amount {ConvertedAmount}...",
                    order.Id, convertedAmount);
                depositTrRs = await depositBridge.DepositAsync(convertedAmount, order.DestinationAddress);
                if (!depositTrRs.IsSuccess)
                {
                    logger.LogWarning(
                        "Deposit transaction failed, initiating abort withdrawal for order {OrderId}...", order.Id);
                    abortTrRs = await withdrawBridge.DepositAsync(order.Amount, virtualAccount.Address);
                    if (!abortTrRs.IsSuccess)
                    {
                        logger.LogError("Abort withdrawal failed: {Error}", abortTrRs.Error.Message);
                        return Result<CheckBalanceResponse>.Failure(abortTrRs.Error);
                    }

                    logger.LogWarning("Deposit transaction failed. Returning error.");
                    return Result<CheckBalanceResponse>.Failure(depositTrRs.Error);
                }

                logger.LogInformation("Retrieving transaction status for deposit transaction...");
                Result<BridgeTransactionStatus> transactionStatus =
                    await depositBridge.GetTransactionStatusAsync(depositTrRs.Value?.TransactionId!, token);
                if (!transactionStatus.IsSuccess)
                {
                    logger.LogError("Failed to retrieve transaction status: {Error}",
                        transactionStatus.Error.Message);
                    return Result<CheckBalanceResponse>.Failure(transactionStatus.Error);
                }

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
                logger.LogInformation("Transaction status for order {OrderId} determined as: {Status}", order.Id,
                    orderStatus);

                order.OrderStatus = orderStatus;
                order.TransactionHash = depositTrRs.Value?.TransactionId;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception occurred during transaction processing for order {OrderId}",
                    order.Id);
                if (withdrawTrRs.IsSuccess && (depositTrRs is null || !depositTrRs.IsSuccess))
                {
                    logger.LogInformation("Attempting to abort withdrawal due to exception for order {OrderId}...",
                        order.Id);
                    abortTrRs = await withdrawBridge.DepositAsync(order.Amount, virtualAccount.Address);
                    if (!abortTrRs.IsSuccess)
                    {
                        logger.LogError("Abort withdrawal failed: {Error}", abortTrRs.Error.Message);
                        return Result<CheckBalanceResponse>.Failure(abortTrRs.Error);
                    }
                }

                return Result<CheckBalanceResponse>.Failure(ResultPatternError.InternalServerError(e.Message));
            }

            logger.LogInformation("Saving updated order status to database for order {OrderId}...", order.Id);
            int res = await dbContext.SaveChangesAsync(token);
            logger.LogInformation("Database save result for order {OrderId}: {Result}", order.Id, res);

            Result<decimal> nowBalance = await withdrawBridge.GetAccountBalanceAsync(virtualAccount.Address, token);
            if (nowBalance.IsSuccess) return Result<CheckBalanceResponse>.Failure(nowBalance.Error);
            return res != 0
                ? Result<CheckBalanceResponse>.Success(new CheckBalanceResponse(
                    order.Id,
                    order.FromNetwork,
                    order.FromToken,
                    nowBalance.Value,
                    order.OrderStatus != OrderStatus.Completed ? order.Amount : 0,
                    order.OrderStatus.ToString(),
                    order.OrderStatus == OrderStatus.Completed ? "Order is already completed." : "",
                    order.TransactionHash))
                : Result<CheckBalanceResponse>.Failure(ResultPatternError.InternalServerError());
        }
        else if (order.OrderStatus == OrderStatus.Pending)
        {
            logger.LogInformation("Retrieving transaction status for deposit transaction...");
            Result<BridgeTransactionStatus> transactionStatus =
                await depositBridge.GetTransactionStatusAsync(order.TransactionHash!, token);
            if (!transactionStatus.IsSuccess)
            {
                logger.LogError("Failed to retrieve transaction status: {Error}",
                    transactionStatus.Error.Message);
                return Result<CheckBalanceResponse>.Failure(transactionStatus.Error);
            }

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
            logger.LogInformation("Transaction status interpreted as: {Status}", orderStatus);

            order.OrderStatus = orderStatus;
            await dbContext.SaveChangesAsync(token);

            return Result<CheckBalanceResponse>.Success(new CheckBalanceResponse(
                order.Id,
                order.FromNetwork,
                order.FromToken,
                balance.Value,
                order.OrderStatus != OrderStatus.Completed ? order.Amount : 0,
                order.OrderStatus.ToString(),
                order.OrderStatus == OrderStatus.Completed ? "Order is already completed." : "",
                order.TransactionHash));
        }

        // Final fallback response if none of the above conditions are met
        logger.LogInformation("Returning default CheckBalanceResponse for order {OrderId}", order.Id);
        return Result<CheckBalanceResponse>.Success(new CheckBalanceResponse(
            order.Id,
            order.FromNetwork,
            order.FromToken,
            balance.Value,
            order.OrderStatus != OrderStatus.Completed ? order.Amount : 0,
            order.OrderStatus.ToString(),
            order.OrderStatus == OrderStatus.Completed ? "Order is already completed." : "",
            order.TransactionHash));
    }


    private bool IsValidSolanaAddress(string address)
        => address.Length == 44 && address.All(char.IsLetterOrDigit);

    private bool IsValidRadixAddress(string address)
        => address.StartsWith("account_tdx_");
}