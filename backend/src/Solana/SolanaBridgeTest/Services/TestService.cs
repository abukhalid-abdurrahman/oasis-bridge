namespace SolanaBridgeTest.Services;

/// <summary>
/// Service for testing interactions with the SolanaBridge API.
/// Includes creating an account, checking balance, restoring an account,
/// making withdrawals, deposits, and checking transaction statuses.
/// Uses SolanaTechnicalAccountBridgeOptions for SolanaBridge configuration.
/// </summary>
public class TestService : ITestService
{
    /// <summary>
    /// Instance of SolanaBridge, initialized with the provided options.
    /// </summary>
    private readonly SolanaBridge.SolanaBridge _bridge;

    /// <summary>
    /// Primary constructor that initializes the SolanaBridge instance with the provided options.
    /// </summary>
    /// <param name="options">Configuration options for the SolanaBridge.</param>
    public TestService(SolanaTechnicalAccountBridgeOptions options) =>
        _bridge = new(options);

    /// <summary>
    /// Asynchronously tests the process of creating a new account on the Solana network.
    /// This includes generating keys and a seed phrase.
    /// </summary>
    public async Task CreateAccountTestAsync()
    {
        try
        {
            // Creating a new account via the SolanaBridge
            (string PublicKey, string PrivateKey, string SeedPhrase) response = await _bridge.CreateAccountAsync();

            // Displaying the generated account details
            Console.WriteLine($"PublicKey: {response.PublicKey}");
            Console.WriteLine($"PrivateKey: {response.PrivateKey}");
            Console.WriteLine($"SeedPhrase: {response.SeedPhrase}");
        }
        catch (Exception ex)
        {
            // Handling errors during account creation
            Console.WriteLine($"Error in CreateAccountTestAsync: {ex.Message}");
        }
    }

    /// <summary>
    /// Asynchronously gets the balance of an account by its address.
    /// </summary>
    /// <param name="accountAddress">The account address for balance retrieval.</param>
    public async Task GetAccountBalanceTestAsync(string accountAddress)
    {
        try
        {
            // Fetching the account balance
            decimal balance = await _bridge.GetAccountBalanceAsync(accountAddress);

            // Displaying the balance in SOL (Solana tokens)
            Console.WriteLine($"Account Balance: {balance} SOL");
        }
        catch (Exception ex)
        {
            // Handling errors while fetching balance
            Console.WriteLine($"Error in GetAccountBalanceTestAsync: {ex.Message}");
        }
    }

    /// <summary>
    /// Asynchronously restores an account using a seed phrase.
    /// </summary>
    /// <param name="seedPhrase">The seed phrase to restore the account.</param>
    public async Task RestoreAccountTestAsync(string seedPhrase)
    {
        try
        {
            // Restoring the account using the provided seed phrase
            (string PublicKey, string PrivateKey) response = await _bridge.RestoreAccountAsync(seedPhrase);

            // Displaying the restored account details
            Console.WriteLine($"Restored PublicKey: {response.PublicKey}");
            Console.WriteLine($"Restored PrivateKey: {response.PrivateKey}");
        }
        catch (Exception ex)
        {
            // Handling errors during account restoration
            Console.WriteLine($"Error in RestoreAccountTestAsync: {ex.Message}");
        }
    }

    /// <summary>
    /// Asynchronously withdraws a specified amount from an account.
    /// </summary>
    /// <param name="amount">The amount to withdraw.</param>
    /// <param name="accountAddress">The account address for withdrawal.</param>
    /// <param name="clientPrivateKey">The client's private key for authorization.</param>
    public async Task WithdrawTestAsync(decimal amount, string accountAddress, string clientPrivateKey)
    {
        try
        {
            // Performing the withdrawal operation
            TransactionResponse transactionResponse =
                await _bridge.WithdrawAsync(amount, accountAddress, clientPrivateKey);

            // Displaying the transaction details
            Console.WriteLine($"TransactionId: {transactionResponse.TransactionId}");
            Console.WriteLine($"ErrorMessage: {transactionResponse.ErrorMessage}");
            Console.WriteLine($"Success: {transactionResponse.Success}");
            Console.WriteLine($"Data: {transactionResponse.Data}");
        }
        catch (Exception ex)
        {
            // Handling errors during withdrawal
            Console.WriteLine($"Error in WithdrawTestAsync: {ex.Message}");
        }
    }

    /// <summary>
    /// Asynchronously deposits a specified amount into an account.
    /// </summary>
    /// <param name="amount">The amount to deposit.</param>
    /// <param name="accountAddress">The account address for deposit.</param>
    public async Task DepositTestAsync(decimal amount, string accountAddress)
    {
        try
        {
            // Performing the deposit operation
            TransactionResponse transactionResponse = await _bridge.DepositAsync(amount, accountAddress);

            // Displaying the transaction details
            Console.WriteLine($"TransactionId: {transactionResponse.TransactionId}");
            Console.WriteLine($"ErrorMessage: {transactionResponse.ErrorMessage}");
            Console.WriteLine($"Success: {transactionResponse.Success}");
            Console.WriteLine($"Data: {transactionResponse.Data}");
        }
        catch (Exception ex)
        {
            // Handling errors during deposit
            Console.WriteLine($"Error in DepositTestAsync: {ex.Message}");
        }
    }

    /// <summary>
    /// Asynchronously gets the status of a transaction by its hash.
    /// </summary>
    /// <param name="transactionHash">The transaction hash to check the status of.</param>
    public async Task GetTransactionStatusTestAsync(string transactionHash)
    {
        try
        {
            // Fetching the transaction status
            BridgeTransactionStatus status = await _bridge.GetTransactionStatusAsync(transactionHash);

            // Displaying the transaction status
            Console.WriteLine($"Transaction Status: {status}");
            Console.WriteLine($"Transaction Confirmations: {status}");
        }
        catch (Exception ex)
        {
            // Handling errors while fetching the transaction status
            Console.WriteLine($"Error in GetTransactionStatusTestAsync: {ex.Message}");
        }
    }
}