using Transaction = Solnet.Rpc.Models.Transaction;

namespace SolanaBridge;

/// <summary>
/// A class representing the SolanaBridge implementation, which enables interaction with the Solana blockchain.
/// Provides methods to retrieve account balances, create and restore accounts, and execute transactions such as withdrawals and deposits.
/// Implements the <see cref="ISolanaBridge"/> interface.
/// </summary>
public sealed class SolanaBridge(SolanaTechnicalAccountBridgeOptions options) : ISolanaBridge
{
    private readonly IRpcClient _rpcClient = ClientFactory.GetClient(options.HostUri);

    /// <summary>
    /// Converts SOL to lamports (the smallest unit in Solana).
    /// </summary>
    /// <param name="amountInSol">The amount in SOL.</param>
    /// <returns>The amount in lamports.</returns>
    private static ulong ConvertSolToLamports(decimal amountInSol) => (ulong)(amountInSol * 1_000_000_000m);

    /// <summary>
    /// Retrieves the balance of a given Solana account in SOL (native currency).
    /// </summary>
    /// <param name="accountAddress">The address of the Solana account.</param>
    /// <param name="token">A token to signal cancellation.</param>
    /// <returns>The balance in SOL as a decimal.</returns>
    public async Task<decimal> GetAccountBalanceAsync(string accountAddress, CancellationToken token = default)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            RequestResult<ResponseValue<AccountInfo>> result = await _rpcClient.GetAccountInfoAsync(accountAddress);
            if (result.WasSuccessful && result.Result.Value?.Lamports != null)
                // Convert lamports to SOL for readability.
                return result.Result.Value.Lamports / 1_000_000_000m;

            return 0m; // Return 0 if no balance found.
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error retrieving balance for account {accountAddress}: {e.Message}");
            throw new ApplicationException("Failed to retrieve account balance.", e);
        }
    }

    /// <summary>
    /// Creates a new Solana account with a seed phrase and retrieves its details.
    /// </summary>
    /// <param name="token">A token to signal cancellation.</param>
    /// <returns>A tuple containing the public key, private key, and seed phrase of the new account.</returns>
    public async Task<(string PublicKey, string PrivateKey, string SeedPhrase)> CreateAccountAsync(
        CancellationToken token = default)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            Mnemonic mnemonic = new(WordList.English, WordCount.Twelve);
            Wallet wallet = new(mnemonic);

            string seedPhrase = string.Join(" ", mnemonic.Words);
            string publicKey = wallet.Account.PublicKey;
            string privateKey = Convert.ToBase64String(wallet.Account.PrivateKey);

            return (publicKey, privateKey, seedPhrase);
        }
        catch (Exception e)
        {
            await Task.CompletedTask;
            Console.WriteLine($"Error creating account: {e.Message}");
            throw new ApplicationException("Failed to create account.", e);
        }
    }

    /// <summary>
    /// Restores a Solana account using a seed phrase.
    /// </summary>
    /// <param name="seedPhrase">The 12-word mnemonic seed phrase.</param>
    /// <param name="token">A token to signal cancellation.</param>
    /// <returns>A tuple containing the public key and private key of the restored account.</returns>
    public async Task<(string PublicKey, string PrivateKey)> RestoreAccountAsync(string seedPhrase,
        CancellationToken token = default)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            if (!SeedPhraseValidator.IsValidSeedPhrase(seedPhrase))
                throw new ArgumentException("Invalid seed phrase.");

            Mnemonic mnemonic = new(seedPhrase);
            Wallet wallet = new(mnemonic);

            string publicKey = wallet.Account.PublicKey;
            string privateKey = Convert.ToBase64String(wallet.Account.PrivateKey);

            return (publicKey, privateKey);
        }
        catch (Exception ex)
        {
            await Task.CompletedTask;
            Console.WriteLine($"Error restoring account: {ex.Message}");
            throw new ApplicationException("Failed to restore account.", ex);
        }
    }

    /// <summary>
    /// Executes a withdrawal from a client account to the technical account.
    /// </summary>
    /// <param name="amount">The amount in SOL to withdraw.</param>
    /// <param name="senderAccountAddress">The public key of the client account.</param>
    /// <param name="senderPrivateKey">The private key of the client account.</param>
    /// <returns>A <see cref="TransactionResponse"/> containing transaction details.</returns>
    public async Task<TransactionResponse> WithdrawAsync(decimal amount, string senderAccountAddress,
        string senderPrivateKey)
    {
        ulong lamports = ConvertSolToLamports(amount);
        Account technicalAccount = new(
            Convert.FromBase64String(options.PrivateKey),
            new PublicKey(options.PublicKey)
        );
        Account clientAccount = new(
            Convert.FromBase64String(senderPrivateKey),
            new PublicKey(senderAccountAddress)
        );

        return await ExecuteTransactionAsync(clientAccount, technicalAccount, lamports);
    }

    /// <summary>
    /// Executes a deposit from the technical account to a client account.
    /// </summary>
    /// <param name="amount">The amount in SOL to deposit.</param>
    /// <param name="receiverAccountAddress">The public key of the client account.</param>
    /// <returns>A <see cref="TransactionResponse"/> containing transaction details.</returns>
    public async Task<TransactionResponse> DepositAsync(decimal amount, string receiverAccountAddress)
    {
        ulong lamports = ConvertSolToLamports(amount);
        Account technicalAccount = new(
            Convert.FromBase64String(options.PrivateKey),
            new PublicKey(options.PublicKey)
        );
        PublicKey receiverAccount = new(receiverAccountAddress);


        return await ExecuteTransactionAsync(technicalAccount, receiverAccount, lamports);
    }

    /// <summary>
    /// Executes a transaction between two accounts.
    /// </summary>
    /// <param name="sender">The sender account.</param>
    /// <param name="receiver">The receiver account.</param>
    /// <param name="lamports">The amount in lamports to transfer.</param>
    /// <returns>A <see cref="TransactionResponse"/> containing transaction details.</returns>
    private async Task<TransactionResponse> ExecuteTransactionAsync(Account sender, PublicKey receiver, ulong lamports)
    {
        try
        {
            RequestResult<ResponseValue<LatestBlockHash>> latestBlockHashResult =
                await _rpcClient.GetLatestBlockHashAsync();

            if (!latestBlockHashResult.WasSuccessful || latestBlockHashResult.Result?.Value == null)
                throw new Exception($"Failed to retrieve latest block hash: {latestBlockHashResult.Reason}");

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
                throw new Exception($"Transaction send error: {result.Reason}");

            return new TransactionResponse(
                result.Result,
                result.Result,
                true,
                null
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing transaction: {ex.Message}");
            throw new ApplicationException("Failed to execute transaction.", ex);
        }
    }

    /// <summary>
    /// Retrieves the status of a Solana transaction using its hash.
    /// </summary>
    /// <param name="transactionHash">The transaction hash to check.</param>
    /// <param name="token">A token to signal cancellation.</param>
    /// <returns>The <see cref="BridgeTransactionStatus"/> of the transaction.</returns>
    public async Task<BridgeTransactionStatus> GetTransactionStatusAsync(string transactionHash,
        CancellationToken token = default)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            RequestResult<TransactionMetaSlotInfo> transactionStatusResult =
                await _rpcClient.GetTransactionAsync(transactionHash, commitment: Commitment.Confirmed);

            if (!transactionStatusResult.WasSuccessful)
                throw new Exception($"Failed to retrieve transaction status: {transactionStatusResult.Reason}");

            TransactionMetaSlotInfo transactionInfo = transactionStatusResult.Result;

            if (transactionInfo == null)
                return BridgeTransactionStatus.NotFound;

            bool isSuccess = transactionInfo.Meta?.Error == null;
            return isSuccess ? BridgeTransactionStatus.Succeed : BridgeTransactionStatus.Failed;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving transaction status: {ex.Message}");
            throw new ApplicationException("Failed to retrieve transaction status.", ex);
        }
    }
}