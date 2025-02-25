namespace SolanaBridge;

/// <summary>
/// A class representing the SolanaBridge implementation, which enables interaction with the Solana blockchain.
/// Provides methods to retrieve account balances, create and restore accounts, and execute transactions such as withdrawals and deposits.
/// Implements the <see cref="ISolanaBridge"/> interface.
/// </summary>
public sealed class SolanaBridge(
    ILogger<SolanaBridge> logger,
    SolanaTechnicalAccountBridgeOptions options,
    IRpcClient rpcClient) : ISolanaBridge
{
    private const decimal Lamports = 1_000_000_000m;

    // Helper method to convert SOL to lamports (the smallest unit in Solana)
    private ulong ConvertSolToLamports(decimal amountInSol)
    {
        logger.LogInformation("Converting {Amount} SOL to lamports.", amountInSol);
        ulong lamportsValue = (ulong)(amountInSol * Lamports);
        logger.LogInformation("Converted value: {Lamports}", lamportsValue);
        return lamportsValue;
    }

    /// <summary>
    /// Retrieves the balance of a given Solana account in SOL.
    /// </summary>
    public async Task<Result<decimal>> GetAccountBalanceAsync(string accountAddress, CancellationToken token = default)
    {
        logger.LogInformation("Starting GetAccountBalanceAsync for account {Account} at {Time}", accountAddress,
            DateTimeOffset.UtcNow);
        try
        {
            token.ThrowIfCancellationRequested();

            logger.LogInformation("Sending RPC request to get account info for {Account}", accountAddress);
            RequestResult<ResponseValue<AccountInfo>> result = await rpcClient.GetAccountInfoAsync(accountAddress);

            if (result.WasSuccessful && result.Result.Value?.Lamports != null)
            {
                decimal balanceInSol = result.Result.Value.Lamports / Lamports;
                logger.LogInformation("Successfully retrieved balance: {Balance} SOL for account {Account}",
                    balanceInSol, accountAddress);
                return Result<decimal>.Success(balanceInSol);
            }
            else
            {
                logger.LogWarning("RPC request was unsuccessful or returned null lamports for account {Account}",
                    accountAddress);
                return Result<decimal>.Success();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving balance for account {Account} at {Time}", accountAddress,
                DateTimeOffset.UtcNow);
            return Result<decimal>.Failure(ResultPatternError.InternalServerError(ex.Message));
        }
        finally
        {
            logger.LogInformation("Finishing GetAccountBalanceAsync for account {Account} at {Time}", accountAddress,
                DateTimeOffset.UtcNow);
        }
    }

    /// <summary>
    /// Creates a new Solana account with a seed phrase and retrieves its details.
    /// </summary>
    public Task<Result<(string PublicKey, string PrivateKey, string SeedPhrase)>> CreateAccountAsync(
        CancellationToken token = default)
    {
        logger.LogInformation("Starting CreateAccountAsync at {Time}", DateTimeOffset.UtcNow);
        try
        {
            token.ThrowIfCancellationRequested();

            logger.LogInformation("Generating mnemonic using 12-word English word list.");
            Mnemonic mnemonic = new(WordList.English, WordCount.Twelve);
            logger.LogInformation("Creating wallet from mnemonic.");
            Wallet wallet = new(mnemonic);

            string seedPhrase = string.Join(" ", mnemonic.Words);
            string publicKey = wallet.Account.PublicKey;
            string privateKey = Convert.ToBase64String(wallet.Account.PrivateKey);

            logger.LogInformation("Account created successfully. PublicKey: {PublicKey}", publicKey);
            return Task.FromResult(Result<(string PublicKey, string PrivateKey, string SeedPhrase)>.Success((publicKey,
                privateKey,
                seedPhrase)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating account at {Time}", DateTimeOffset.UtcNow);
            return Task.FromResult(Result<(string PublicKey, string PrivateKey, string SeedPhrase)>.Failure(
                ResultPatternError.InternalServerError(ex.Message)));
        }
        finally
        {
            logger.LogInformation("Finishing CreateAccountAsync at {Time}", DateTimeOffset.UtcNow);
        }
    }

    /// <summary>
    /// Restores a Solana account using a seed phrase.
    /// </summary>
    public Task<Result<(string PublicKey, string PrivateKey)>> RestoreAccountAsync(string seedPhrase,
        CancellationToken token = default)
    {
        logger.LogInformation("Starting RestoreAccountAsync at {Time}", DateTimeOffset.UtcNow);
        try
        {
            token.ThrowIfCancellationRequested();

            if (!SeedPhraseValidator.IsValidSeedPhrase(seedPhrase))
            {
                logger.LogWarning("Invalid seed phrase format provided.");
                return Task.FromResult(Result<(string PublicKey, string PrivateKey)>.Failure(
                    ResultPatternError.BadRequest("Invalid format seed phrase.")));
            }

            logger.LogInformation("Creating mnemonic from provided seed phrase.");
            Mnemonic mnemonic = new(seedPhrase);
            logger.LogInformation("Creating wallet from mnemonic.");
            Wallet wallet = new(mnemonic);

            string publicKey = wallet.Account.PublicKey;
            string privateKey = Convert.ToBase64String(wallet.Account.PrivateKey);

            logger.LogInformation("Account restored successfully. PublicKey: {PublicKey}", publicKey);
            return Task.FromResult(Result<(string PublicKey, string PrivateKey)>.Success((publicKey, privateKey)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error restoring account at {Time}", DateTimeOffset.UtcNow);
            return Task.FromResult(Result<(string PublicKey, string PrivateKey)>.Failure(
                ResultPatternError.InternalServerError(ex.Message)));
        }
        finally
        {
            logger.LogInformation("Finishing RestoreAccountAsync at {Time}", DateTimeOffset.UtcNow);
        }
    }

    /// <summary>
    /// Executes a withdrawal from a client account to the technical account.
    /// </summary>
    public async Task<Result<TransactionResponse>> WithdrawAsync(decimal amount, string senderAccountAddress,
        string senderPrivateKey)
    {
        logger.LogInformation("Starting WithdrawAsync for account {Account} at {Time}", senderAccountAddress,
            DateTimeOffset.UtcNow);
        if (senderPrivateKey == options.PublicKey)
        {
            logger.LogWarning("Withdrawal attempted from technical account. Operation not allowed.");
            return Result<TransactionResponse>.Failure(
                ResultPatternError.BadRequest("Transaction from tech account to the same account is not allowed."));
        }

        ulong lamports = ConvertSolToLamports(amount);
        logger.LogInformation("Converted {Amount} SOL to {Lamports} lamports.", amount, lamports);

        logger.LogInformation("Preparing technical account and client account instances.");
        Account technicalAccount = new(
            Convert.FromBase64String(options.PrivateKey),
            new PublicKey(options.PublicKey)
        );
        Account clientAccount = new(
            Convert.FromBase64String(senderPrivateKey),
            new PublicKey(senderAccountAddress)
        );

        logger.LogInformation("Finishing WithdrawAsync initial steps at {Time}", DateTimeOffset.UtcNow);
        return await ExecuteTransactionAsync(clientAccount, technicalAccount, lamports);
    }

    /// <summary>
    /// Executes a deposit from the technical account to a client account.
    /// </summary>
    public async Task<Result<TransactionResponse>> DepositAsync(decimal amount, string receiverAccountAddress)
    {
        logger.LogInformation("Starting DepositAsync for account {Account} at {Time}", receiverAccountAddress,
            DateTimeOffset.UtcNow);
        if (receiverAccountAddress == options.PublicKey)
        {
            logger.LogWarning("Deposit attempted to technical account. Operation not allowed.");
            return Result<TransactionResponse>.Failure(
                ResultPatternError.BadRequest("Transaction from tech account to the same account is not allowed."));
        }

        ulong lamports = ConvertSolToLamports(amount);
        logger.LogInformation("Converted {Amount} SOL to {Lamports} lamports.", amount, lamports);

        logger.LogInformation("Preparing technical account and receiver account instance.");
        Account technicalAccount = new(
            Convert.FromBase64String(options.PrivateKey),
            new PublicKey(options.PublicKey)
        );
        PublicKey receiverAccount = new(receiverAccountAddress);

        logger.LogInformation("Finishing DepositAsync initial steps at {Time}", DateTimeOffset.UtcNow);
        return await ExecuteTransactionAsync(technicalAccount, receiverAccount, lamports);
    }


    /// <summary>
    /// Executes a transaction between two accounts.
    /// </summary>
    private async Task<Result<TransactionResponse>> ExecuteTransactionAsync(Account sender, PublicKey receiver,
        ulong lamports)
    {
        try
        {
            logger.LogInformation("Starting ExecuteTransactionAsync at {Time}", DateTimeOffset.UtcNow);

            // Retrieve sender account balance in SOL
            logger.LogInformation("Retrieving balance for sender account {Account}.", sender.PublicKey);
            Result<decimal> balanceResult = await GetAccountBalanceAsync(sender.PublicKey);
            if (!balanceResult.IsSuccess)
            {
                logger.LogError("Failed to retrieve balance for account {Account}.", sender.PublicKey);
                return Result<TransactionResponse>.Failure(balanceResult.Error);
            }

            // Check if the lamports amount is valid relative to the account balance
            decimal balanceSol = balanceResult.Value;
            logger.LogInformation("Sender balance: {Balance} SOL", balanceSol);
            if ((lamports / Lamports) >= balanceSol)
            {
                logger.LogWarning("Insufficient funds: {Lamports} lamports requested exceeds balance {Balance} SOL.",
                    lamports, balanceSol);
                return Result<TransactionResponse>.Failure(
                    ResultPatternError.BadRequest("Amount is too small to be included."),
                    new TransactionResponse("", null, false, "Invalid amount",
                        BridgeTransactionStatus.InsufficientFunds));
            }

            // Retrieve the latest block hash
            logger.LogInformation("Requesting latest block hash from RPC client.");
            RequestResult<ResponseValue<LatestBlockHash>> latestBlockHashResult =
                await rpcClient.GetLatestBlockHashAsync();
            if (!latestBlockHashResult.WasSuccessful || latestBlockHashResult.Result?.Value == null)
            {
                string errorMsg = $"Failed to retrieve latest block hash: {latestBlockHashResult.Reason}";
                logger.LogError(errorMsg);
                return Result<TransactionResponse>.Failure(ResultPatternError.InternalServerError(errorMsg));
            }

            string recentBlockHash = latestBlockHashResult.Result.Value.Blockhash;
            logger.LogInformation("Received latest block hash: {BlockHash}", recentBlockHash);

            // Build the transaction
            logger.LogInformation(
                "Building transaction for transfer of {Lamports} lamports from {Sender} to {Receiver}.", lamports,
                sender.PublicKey, receiver);
            Transaction transaction = new()
            {
                RecentBlockHash = recentBlockHash,
                FeePayer = sender.PublicKey
            };

            transaction.Add(SystemProgram.Transfer(sender.PublicKey, receiver, lamports));
            logger.LogInformation("Transaction constructed. Signing transaction with sender's key.");

            // Sign the transaction with the sender's key
            transaction.Sign(sender);

            logger.LogInformation("Transaction signed. Sending transaction to the network.");
            RequestResult<string> result = await rpcClient.SendTransactionAsync(transaction.Serialize());
            if (!result.WasSuccessful)
            {
                logger.LogError("Transaction submission failed with status {Status} and reason: {Reason}",
                    result.HttpStatusCode, result.Reason);
                return result.HttpStatusCode switch
                {
                    HttpStatusCode.InternalServerError => Result<TransactionResponse>.Failure(
                        ResultPatternError.InternalServerError($"Transaction send error: {result.Reason}")),
                    HttpStatusCode.BadRequest => Result<TransactionResponse>.Failure(
                        ResultPatternError.BadRequest($"Transaction send error: {result.Reason}")),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            logger.LogInformation("Transaction submitted successfully. Transaction hash: {TxHash}", result.Result);
            return Result<TransactionResponse>.Success(new TransactionResponse(
                result.Result,
                result.Result,
                true,
                null,
                BridgeTransactionStatus.Completed
            ));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing transaction at {Time}", DateTimeOffset.UtcNow);
            return Result<TransactionResponse>.Failure(ResultPatternError.InternalServerError(ex.Message));
        }
        finally
        {
            logger.LogInformation("Finishing ExecuteTransactionAsync at {Time}", DateTimeOffset.UtcNow);
        }
    }

    /// <summary>
    /// Retrieves the status of a Solana transaction using its hash.
    /// </summary>
    public async Task<Result<BridgeTransactionStatus>> GetTransactionStatusAsync(string transactionHash,
        CancellationToken token = default)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            logger.LogInformation("Fetching transaction status for {TxHash}", transactionHash);
            
            Commitment commitment = Commitment.Confirmed;
            RequestResult<TransactionMetaSlotInfo> transactionStatusResult =
                await rpcClient.GetTransactionAsync(transactionHash, commitment);

            if (transactionStatusResult == null)
            {
                logger.LogError("Transaction status response is null for {TxHash}", transactionHash);
                return Result<BridgeTransactionStatus>.Failure(
                    ResultPatternError.InternalServerError("Transaction status retrieval failed."));
            }

            if (!transactionStatusResult.WasSuccessful || transactionStatusResult.Result == null)
            {
                logger.LogError(
                    "Failed to retrieve transaction status for {TxHash}. Response was unsuccessful. Raw response: {RawResponse}",
                    transactionHash, transactionStatusResult.RawRpcResponse ?? "null");
                if(transactionStatusResult.HttpStatusCode==HttpStatusCode.BadRequest)
                return Result<BridgeTransactionStatus>.Failure(
                    ResultPatternError.InternalServerError("Transaction status retrieval failed."));
            }

            logger.LogInformation("Transaction status response: {@Response}", transactionStatusResult);

            BridgeTransactionStatus status = transactionStatusResult.Result.Meta?.Error == null
                ? BridgeTransactionStatus.Completed
                : BridgeTransactionStatus.Canceled;
            return Result<BridgeTransactionStatus>.Success(status);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred while retrieving transaction status for {TxHash}", transactionHash);
            return Result<BridgeTransactionStatus>.Failure(
                ResultPatternError.InternalServerError("Unexpected error occurred."));
        }
    }
}