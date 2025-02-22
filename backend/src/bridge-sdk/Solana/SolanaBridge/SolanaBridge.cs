using Transaction = Solnet.Rpc.Models.Transaction;

namespace SolanaBridge;

/// <summary>
/// A class representing the SolanaBridge implementation, which enables interaction with the Solana blockchain.
/// Provides methods to retrieve account balances, create and restore accounts, and execute transactions such as withdrawals and deposits.
/// Implements the <see cref="ISolanaBridge"/> interface.
/// </summary>
public sealed class SolanaBridge(
    ILogger<SolanaBridge> logger,
    SolanaTechnicalAccountBridgeOptions options) : ISolanaBridge
{
    private readonly IRpcClient _rpcClient = ClientFactory.GetClient(options.HostUri);
    private const decimal Lamports = 1_000_000_000m;

    /// <summary>
    /// Converts SOL to lamports (the smallest unit in Solana).
    /// </summary>
    /// <param name="amountInSol">The amount in SOL.</param>
    /// <returns>The amount in lamports.</returns>
    private static ulong ConvertSolToLamports(decimal amountInSol)
        => (ulong)(amountInSol * Lamports);

    /// <summary>
    /// Retrieves the balance of a given Solana account in SOL (native currency).
    /// </summary>
    /// <param name="accountAddress">The address of the Solana account.</param>
    /// <param name="token">A token to signal cancellation.</param>
    /// <returns>The balance in SOL as a decimal.</returns>
    public async Task<Result<decimal>> GetAccountBalanceAsync(string accountAddress, CancellationToken token = default)
    {
        try
        {
            logger.LogInformation($"Starting method to GetAccountBalanceAsync in time: {DateTimeOffset.UtcNow};");
            token.ThrowIfCancellationRequested();
            RequestResult<ResponseValue<AccountInfo>> result = await _rpcClient.GetAccountInfoAsync(accountAddress);
            if (result.WasSuccessful && result.Result.Value?.Lamports != null)
                // Convert lamports to SOL for readability.
                return Result<decimal>.Success(result.Result.Value.Lamports / Lamports);

            logger.LogInformation($"Finishing method to GetAccountBalanceAsync in time: {DateTimeOffset.UtcNow};");

            return Result<decimal>.Failure(
                ResultPatternError.NotFound("Account not found"));
        }
        catch (Exception e)
        {
            logger.LogError(
                $"Error retrieving balance for account {accountAddress}: {e.Message}, in time:{DateTimeOffset.UtcNow}");
            return Result<decimal>.Failure(
                ResultPatternError.InternalServerError(e.Message));
        }
    }

    /// <summary>
    /// Creates a new Solana account with a seed phrase and retrieves its details.
    /// </summary>
    /// <param name="token">A token to signal cancellation.</param>
    /// <returns>A tuple containing the public key, private key, and seed phrase of the new account.</returns>
    public async Task<Result<(string PublicKey, string PrivateKey, string SeedPhrase)>> CreateAccountAsync(
        CancellationToken token = default)
    {
        try
        {
            logger.LogInformation($"Starting method to CreateAccountAsync in time: {DateTimeOffset.UtcNow};");
            token.ThrowIfCancellationRequested();
            Mnemonic mnemonic = new(WordList.English, WordCount.Twelve);
            Wallet wallet = new(mnemonic);

            string seedPhrase = string.Join(" ", mnemonic.Words);
            string publicKey = wallet.Account.PublicKey;
            string privateKey = Convert.ToBase64String(wallet.Account.PrivateKey);

            logger.LogInformation($"Finishing method to CreateAccountAsync in time: {DateTimeOffset.UtcNow};");

            return Result<(string PublicKey, string PrivateKey, string SeedPhrase)>
                .Success(new(publicKey, privateKey, seedPhrase));
        }
        catch (Exception e)
        {
            await Task.CompletedTask;
            logger.LogError($"Error creating account: {e.Message},time:{DateTimeOffset.UtcNow}");
            return Result<(string PublicKey, string PrivateKey, string SeedPhrase)>
                .Failure(ResultPatternError.InternalServerError(e.Message));
        }
    }

    /// <summary>
    /// Restores a Solana account using a seed phrase.
    /// </summary>
    /// <param name="seedPhrase">The 12-word mnemonic seed phrase.</param>
    /// <param name="token">A token to signal cancellation.</param>
    /// <returns>A tuple containing the public key and private key of the restored account.</returns>
    public async Task<Result<(string PublicKey, string PrivateKey)>> RestoreAccountAsync(string seedPhrase,
        CancellationToken token = default)
    {
        try
        {
            logger.LogInformation($"Starting method to RestoreAccountAsync in time: {DateTimeOffset.UtcNow};");

            token.ThrowIfCancellationRequested();
            if (!SeedPhraseValidator.IsValidSeedPhrase(seedPhrase))
                Result<(string PublicKey, string PrivateKey)>.Failure(
                    ResultPatternError.InternalServerError("Invalid seed phrase."));

            Mnemonic mnemonic = new(seedPhrase);
            Wallet wallet = new(mnemonic);

            string publicKey = wallet.Account.PublicKey;
            string privateKey = Convert.ToBase64String(wallet.Account.PrivateKey);

            logger.LogInformation($"Finishing method to RestoreAccountAsync in time: {DateTimeOffset.UtcNow};");

            return Result<(string PublicKey, string PrivateKey)>
                .Success(new(publicKey, privateKey));
        }
        catch (Exception ex)
        {
            await Task.CompletedTask;
            logger.LogError($"Error restoring account: {ex.Message}, time:{DateTimeOffset.UtcNow}");
            return Result<(string PublicKey, string PrivateKey)>.Failure(
                ResultPatternError.InternalServerError(ex.Message));
        }
    }

    /// <summary>
    /// Executes a withdrawal from a client account to the technical account.
    /// </summary>
    /// <param name="amount">The amount in SOL to withdraw.</param>
    /// <param name="senderAccountAddress">The public key of the client account.</param>
    /// <param name="senderPrivateKey">The private key of the client account.</param>
    /// <returns>A <see cref="TransactionResponse"/> containing transaction details.</returns>
    public async Task<Result<TransactionResponse>> WithdrawAsync(decimal amount, string senderAccountAddress,
        string senderPrivateKey)
    {
        logger.LogInformation($"Starting method to WithdrawAsync in time: {DateTimeOffset.UtcNow};");

        ulong lamports = ConvertSolToLamports(amount);
        Account technicalAccount = new(
            Convert.FromBase64String(options.PrivateKey),
            new PublicKey(options.PublicKey)
        );
        Account clientAccount = new(
            Convert.FromBase64String(senderPrivateKey),
            new PublicKey(senderAccountAddress)
        );

        logger.LogInformation($"Finishing method to WithdrawAsync in time: {DateTimeOffset.UtcNow};");

        return await ExecuteTransactionAsync(clientAccount, technicalAccount, lamports);
    }

    /// <summary>
    /// Executes a deposit from the technical account to a client account.
    /// </summary>
    /// <param name="amount">The amount in SOL to deposit.</param>
    /// <param name="receiverAccountAddress">The public key of the client account.</param>
    /// <returns>A <see cref="TransactionResponse"/> containing transaction details.</returns>
    public async Task<Result<TransactionResponse>> DepositAsync(decimal amount, string receiverAccountAddress)
    {
        logger.LogInformation($"Starting method to DepositAsync in time: {DateTimeOffset.UtcNow};");

        ulong lamports = ConvertSolToLamports(amount);
        Account technicalAccount = new(
            Convert.FromBase64String(options.PrivateKey),
            new PublicKey(options.PublicKey)
        );
        PublicKey receiverAccount = new(receiverAccountAddress);

        logger.LogInformation($"Finishing method to DepositAsync in time: {DateTimeOffset.UtcNow};");

        return await ExecuteTransactionAsync(technicalAccount, receiverAccount, lamports);
    }

    /// <summary>
    /// Executes a transaction between two accounts.
    /// </summary>
    /// <param name="sender">The sender account.</param>
    /// <param name="receiver">The receiver account.</param>
    /// <param name="lamports">The amount in lamports to transfer.</param>
    /// <returns>A <see cref="TransactionResponse"/> containing transaction details.</returns>
    private async Task<Result<TransactionResponse>> ExecuteTransactionAsync(Account sender, PublicKey receiver,
        ulong lamports)
    {
        try
        {
            logger.LogInformation($"Starting method to ExecuteTransactionAsync in time: {DateTimeOffset.UtcNow};");

            Result<decimal> getAccountBalanceRes = await GetAccountBalanceAsync(sender.PublicKey);

            if (!getAccountBalanceRes.IsSuccess)
                return Result<TransactionResponse>.Failure(
                    ResultPatternError.InternalServerError("Error in getAccountBalance"));

            if (lamports / Lamports > getAccountBalanceRes.Value)
                return Result<TransactionResponse>.Failure(
                    ResultPatternError.BadRequest("Amount is too small to be included ."), new(
                        "",
                        null,
                        false,
                        "Invalid amount",
                        BridgeTransactionStatus.InsufficientFunds));


            RequestResult<ResponseValue<LatestBlockHash>> latestBlockHashResult =
                await _rpcClient.GetLatestBlockHashAsync();

            if (!latestBlockHashResult.WasSuccessful || latestBlockHashResult.Result?.Value == null)
                return Result<TransactionResponse>.Failure(ResultPatternError
                    .InternalServerError($"Failed to retrieve latest block hash: {latestBlockHashResult.Reason}"));

            string recentBlockHash = latestBlockHashResult.Result.Value.Blockhash;

            Transaction transaction = new()
            {
                RecentBlockHash = recentBlockHash,
                FeePayer = sender.PublicKey
            };

            transaction.Add(SystemProgram.Transfer(
                sender.PublicKey,
                receiver,
                lamports
            ));

            transaction.Sign(sender);

            RequestResult<string> result = await _rpcClient.SendTransactionAsync(transaction.Serialize());
            if (!result.WasSuccessful)
                return Result<TransactionResponse>.Failure(ResultPatternError
                    .InternalServerError($"Transaction send error: {result.Reason}"));

            logger.LogInformation($"Finishing method to ExecuteTransactionAsync in time: {DateTimeOffset.UtcNow};");


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
            logger.LogError($"Error executing transaction: {ex.Message},time:{DateTimeOffset.UtcNow}");
            return Result<TransactionResponse>.Failure(ResultPatternError
                .InternalServerError(ex.Message));
        }
    }

    /// <summary>
    /// Retrieves the status of a Solana transaction using its hash.
    /// </summary>
    /// <param name="transactionHash">The transaction hash to check.</param>
    /// <param name="token">A token to signal cancellation.</param>
    /// <returns>The <see cref="BridgeTransactionStatus"/> of the transaction.</returns>
    public async Task<Result<BridgeTransactionStatus>> GetTransactionStatusAsync(string transactionHash,
        CancellationToken token = default)
    {
        try
        {
            logger.LogInformation($"Starting method to GetTransactionStatusAsync in time: {DateTimeOffset.UtcNow};");

            token.ThrowIfCancellationRequested();
            RequestResult<TransactionMetaSlotInfo> transactionStatusResult =
                await _rpcClient.GetTransactionAsync(transactionHash, commitment: Commitment.Confirmed);

            if (!transactionStatusResult.WasSuccessful)
                Result<BridgeTransactionStatus>.Failure(
                    ResultPatternError.InternalServerError(
                        $"Failed to retrieve transaction status: {transactionStatusResult.Reason},time:{DateTimeOffset.UtcNow}"));

            TransactionMetaSlotInfo transactionInfo = transactionStatusResult.Result;

            if (transactionInfo == null)
                return Result<BridgeTransactionStatus>.Failure(
                    ResultPatternError.BadRequest("Transaction not found!"), BridgeTransactionStatus.NotFound);

            logger.LogInformation($"Finishing method to GetTransactionStatusAsync in time: {DateTimeOffset.UtcNow};");

            return transactionInfo.Meta?.Error?.Type switch
            {
                TransactionErrorType.AccountNotFound => Result<BridgeTransactionStatus>.Failure(
                    ResultPatternError.BadRequest(), BridgeTransactionStatus.NotFound),
                TransactionErrorType.InsufficientFundsForFee => Result<BridgeTransactionStatus>.Failure(
                    ResultPatternError.BadRequest(), BridgeTransactionStatus.InsufficientFundsForFee),
                _ => Result<BridgeTransactionStatus>.Success(BridgeTransactionStatus.Completed)
            };
        }
        catch (Exception ex)
        {
            logger.LogError($"Error retrieving transaction status: {ex.Message},time:{DateTimeOffset.UtcNow}");
            return Result<BridgeTransactionStatus>.Failure(
                ResultPatternError.InternalServerError(
                    $"Failed to retrieve transaction status.ErrorMessage: \n{ex.Message}"));
        }
    }
}