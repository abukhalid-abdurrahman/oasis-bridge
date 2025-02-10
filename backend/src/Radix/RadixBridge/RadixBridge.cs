using TransactionBuilder = RadixEngineToolkit.TransactionBuilder;

namespace RadixBridge;

/// <summary>
/// Represents a bridge for interacting with the Radix network.
/// This class provides functionality to manage accounts, check balances, 
/// and execute transactions (deposit/withdraw) on the Radix network.
/// </summary>
public sealed class RadixBridge : IRadixBridge
{
    private readonly RadixTechnicalAccountBridgeOptions _options; // Options to configure the bridge
    private readonly HttpClient _httpClient; // HTTP client used to make requests to Radix API
    private readonly string _network; // Name of the network (MainNet or StokeNet)
    private readonly Address _xrdAddress; // Address associated with XRD resources

    /// <summary>
    /// Initializes a new instance of the <see cref="RadixBridge"/> class.
    /// Sets the network and XRD address based on the provided options.
    /// </summary>
    /// <param name="options">The options for configuring the Radix bridge.</param>
    /// <param name="httpClient">The HTTP client used for making requests.</param>
    public RadixBridge(RadixTechnicalAccountBridgeOptions options, HttpClient httpClient)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options)); // Ensure options are provided
        _httpClient =
            httpClient ?? throw new ArgumentNullException(nameof(httpClient)); // Ensure HttpClient is provided

        // Set the network and XRD address based on the provided network ID
        if (options.NetworkId == (byte)NetworkType.Main)
        {
            _network = RadixBridgeHelper.MainNet; // Use MainNet for the main network
            _xrdAddress = new(RadixBridgeHelper.MainNetXrdAddress); // Set the address for MainNet
        }
        else
        {
            _network = RadixBridgeHelper.StokeNet; // Use StokeNet for the test network
            _xrdAddress = new(RadixBridgeHelper.StokeNetXrdAddress); // Set the address for StokeNet
        }
    }

    /// <summary>
    /// Gets the balance of an account.
    /// </summary>
    /// <param name="accountAddress">The address of the account to check.</param>
    /// <param name="token">A cancellation token to cancel the request if needed.</param>
    /// <returns>The account balance as a decimal.</returns>
    public async Task<decimal> GetAccountBalanceAsync(string accountAddress, CancellationToken token = default)
    {
        var data = new
        {
            network = _network,
            account_address = accountAddress, // The account address to check balance for
            resource_address = _xrdAddress.AddressString() // The XRD address for the resource
        };

        // Send a request to the Radix API to get the account's fungible resource balance
        AccountFungibleResourceBalanceDto? result =
            await RadixHttpClientHelper.PostAsync<object, AccountFungibleResourceBalanceDto>(
                _httpClient,
                "https://stokenet-core.radix.live/core/lts/state/account-fungible-resource-balance",
                data,
                token
            );

        // Return the balance if the result is valid, otherwise return 0
        return result != null ? decimal.Parse(result.FungibleResourceBalance.Amount) : 0;
    }

    /// <summary>
    /// Creates a new Radix account with a randomly generated seed phrase.
    /// </summary>
    /// <param name="token">A cancellation token to cancel the request if needed.</param>
    /// <returns>A tuple containing the public key, private key, and seed phrase of the new account.</returns>
    public (PublicKey PublicKey, PrivateKey PrivateKey, string SeedPhrase) CreateAccountAsync(
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested(); // Check if cancellation has been requested

        // Generate a new mnemonic (seed phrase) using a word list
        Mnemonic mnemonic = new(Wordlist.English, WordCount.TwentyFour);
        // Derive the private key from the mnemonic
        PrivateKey privateKey = RadixBridgeHelper.GetPrivateKey(mnemonic);

        // Return the public key, private key, and the seed phrase
        return (privateKey.PublicKey(), privateKey, mnemonic.ToString());
    }

    /// <summary>
    /// Restores an account using a seed phrase.
    /// </summary>
    /// <param name="seedPhrase">The seed phrase of the account to restore.</param>
    /// <param name="token">A cancellation token to cancel the request if needed.</param>
    /// <returns>A tuple containing the public key and private key of the restored account.</returns>
    public (PublicKey PublicKey, PrivateKey PrivateKey) RestoreAccountAsync(string seedPhrase,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested(); // Check if cancellation has been requested

        // Validate the provided seed phrase before proceeding
        if (!SeedPhraseValidator.IsValidSeedPhrase(seedPhrase))
        {
            throw new ArgumentException("SeedPhrase is invalid.");
        }

        // Create a new mnemonic from the provided seed phrase
        Mnemonic mnemonic = new(seedPhrase);
        // Derive the private key from the mnemonic
        PrivateKey privateKey = RadixBridgeHelper.GetPrivateKey(mnemonic);

        // Return the public key and private key
        return (privateKey.PublicKey(), privateKey);
    }

    /// <summary>
    /// Gets the address of an account or identity based on the public key, address type, and network type.
    /// </summary>
    /// <param name="publicKey">The public key associated with the account or identity.</param>
    /// <param name="addressType">The type of address (Account or Identity).</param>
    /// <param name="networkType">The type of network (Main or Stoke).</param>
    /// <param name="token">A cancellation token to cancel the request if needed.</param>
    /// <returns>The address string corresponding to the given public key and network type.</returns>
    public string GetAddressAsync(PublicKey publicKey, AddressType addressType, NetworkType networkType,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested(); // Check if cancellation has been requested

        byte network = (byte)networkType; // Convert network type to byte

        // Return the address string based on the address type (Account or Identity)
        return addressType switch
        {
            AddressType.Account => Address.VirtualAccountAddressFromPublicKey(publicKey, network).AddressString(),
            AddressType.Identity => Address.VirtualIdentityAddressFromPublicKey(publicKey, network).AddressString(),
            _ => throw new ArgumentOutOfRangeException(nameof(addressType), addressType, null) // Invalid address type
        };
    }

    /// <summary>
    /// Withdraws a specified amount from an account.
    /// </summary>
    /// <param name="amount">The amount to withdraw.</param>
    /// <param name="senderAccountAddress">The address of the sender account.</param>
    /// <param name="senderPrivateKey">The private key of the sender account.</param>
    /// <returns>A task representing the transaction response.</returns>
    public async Task<TransactionResponse> WithdrawAsync(decimal amount, string senderAccountAddress,
        string senderPrivateKey)
        => await ExecuteTransactionAsync(amount, senderAccountAddress, senderPrivateKey, true);

    /// <summary>
    /// Deposits a specified amount to an account.
    /// </summary>
    /// <param name="amount">The amount to deposit.</param>
    /// <param name="receiverAccountAddress">The address of the receiver account.</param>
    /// <returns>A task representing the transaction response.</returns>
    public async Task<TransactionResponse> DepositAsync(decimal amount, string receiverAccountAddress)
        => await ExecuteTransactionAsync(amount, receiverAccountAddress, _options.PrivateKey, false);

    /// <summary>
    /// Executes a transaction (withdrawal or deposit) by building and submitting the transaction manifest.
    /// </summary>
    /// <param name="amount">The amount involved in the transaction.</param>
    /// <param name="accountAddress">The account address (sender or receiver).</param>
    /// <param name="privateKey">The private key associated with the account.</param>
    /// <param name="isWithdraw">Indicates whether the transaction is a withdrawal (true) or deposit (false).</param>
    /// <returns>A task representing the transaction response.</returns>
    private async Task<TransactionResponse> ExecuteTransactionAsync(decimal amount, string accountAddress,
        string privateKey, bool isWithdraw)
    {
        using PrivateKey
            senderPrivateKey =
                new(Encoders.Hex.DecodeData(privateKey), Curve.ED25519); // Create a private key from the provided hex
        using Address sender =
            Address.VirtualAccountAddressFromPublicKey(senderPrivateKey.PublicKey(),
                _options.NetworkId); // Derive sender's address

        Address receiver = new(accountAddress); // Set the receiver's address
        if (isWithdraw)
            receiver = new(_options.AccountAddress); // For withdrawal, the receiver is the account address from options

        // Build the transaction manifest with the necessary actions (lock fee, withdraw, take resource, deposit)
        using TransactionManifest manifest = new ManifestBuilder()
            .AccountLockFeeAndWithdraw(sender, new("10"), _xrdAddress, new($"{amount}"))
            .TakeFromWorktop(_xrdAddress, new($"{amount}"), new("xrdBucket"))
            .AccountTryDepositOrAbort(receiver, new("xrdBucket"), null)
            .Build(_options.NetworkId);

        manifest.StaticallyValidate(); // Validate the manifest before execution

        // Get the current epoch for the transaction header
        ulong currentEpoch = (await _httpClient.GetConstructionMetadata(_network))?.CurrentEpoch ?? 0;

        using NotarizedTransaction transaction = new TransactionBuilder()
            .Header(new TransactionHeader(
                networkId: _options.NetworkId,
                startEpochInclusive: currentEpoch,
                endEpochExclusive: currentEpoch + 50,
                nonce: RadixBridgeHelper.RandomNonce(),
                notaryPublicKey: senderPrivateKey.PublicKey(),
                notaryIsSignatory: true,
                tipPercentage: 0
            ))
            .Manifest(manifest) // Attach the manifest
            .Message(new Message.None()) // No additional message for the transaction
            .NotarizeWithPrivateKey(senderPrivateKey); // Notarize the transaction with the sender's private key

        // Prepare the data to send to the Radix API
        var data = new
        {
            network = _network,
            notarized_transaction_hex = Encoders.Hex.EncodeData(transaction.Compile()), // Compile the transaction
            force_recalculate = true // Flag to force recalculation
        };

        // Submit the transaction to the Radix network
        TransactionSubmitResponse? response = await RadixHttpClientHelper.PostAsync<object, TransactionSubmitResponse>(
            _httpClient,
            "https://stokenet-core.radix.live/core/lts/transaction/submit",
            data
        );

        // Return the transaction response, including intent hash and status
        return new(
            transaction.IntentHash().AsStr(),
            response?.Duplicate.ToString(),
            response != null, // If response is null, the transaction failed
            response == null ? "Transaction failed" : null
        );
    }

    /// <summary>
    /// Gets the status of a submitted transaction.
    /// </summary>
    /// <param name="transactionHash">The hash of the transaction to check status for.</param>
    /// <param name="token">A cancellation token to cancel the request if needed.</param>
    /// <returns>The status of the transaction (Succeed, Failed, or NotFound).</returns>
    public async Task<BridgeTransactionStatus> GetTransactionStatusAsync(string transactionHash,
        CancellationToken token = default)
    {
        var data = new
        {
            network = _network,
            intent_hash = transactionHash // Provide the transaction hash to check status
        };

        // Send a request to get the status of the transaction
        TransactionStatusResponse? response = await RadixHttpClientHelper.PostAsync<object, TransactionStatusResponse>(
            _httpClient,
            "https://stokenet-core.radix.live/core/transaction/status",
            data,
            token
        );

        // Return the transaction status based on the response
        return response?.IntentStatus switch
        {
            RadixTransactionStatus.CommittedSuccess => BridgeTransactionStatus.Succeed, // Success
            RadixTransactionStatus.CommittedFailure => BridgeTransactionStatus.Failed, // Failure
            _ => BridgeTransactionStatus.NotFound // Transaction not found
        };
    }

    #region Implementation for contract IBridge

    /// <summary>
    /// Creates a new account asynchronously using a randomly generated seed phrase.
    /// </summary>
    /// <param name="token">A cancellation token to support task cancellation.</param>
    /// <returns>A tuple containing the public key, private key, and the generated seed phrase.</returns>
    async Task<(string PublicKey, string PrivateKey, string SeedPhrase)> IBridge.CreateAccountAsync(
        CancellationToken token)
    {
        return await Task.Run(() =>
        {
            // Check if cancellation has been requested, and throw if it has
            token.ThrowIfCancellationRequested();

            // Generate a new mnemonic (seed phrase) using an English word list with 24 words
            Mnemonic mnemonic = new(Wordlist.English, WordCount.TwentyFour);

            // Derive the private key from the generated mnemonic
            using PrivateKey privateKey = RadixBridgeHelper.GetPrivateKey(mnemonic);

            // Convert the derived public key to a hexadecimal string for easy representation
            string publicKey = Encoders.Hex.EncodeData(privateKey.PublicKeyBytes());

            // Return a tuple containing the public key, private key, and the generated mnemonic (seed phrase)
            return (publicKey, privateKey.RawHex(), mnemonic.ToString());
        }, token);
    }

    /// <summary>
    /// Restores an account asynchronously using a provided seed phrase.
    /// </summary>
    /// <param name="seedPhrase">The seed phrase used to restore the account.</param>
    /// <param name="token">A cancellation token to support task cancellation.</param>
    /// <returns>A tuple containing the public key and private key restored from the seed phrase.</returns>
    /// <exception cref="ArgumentException">Thrown when the seed phrase is invalid.</exception>
    async Task<(string PublicKey, string PrivateKey)> IBridge.RestoreAccountAsync(string seedPhrase,
        CancellationToken token)
    {
        return await Task.Run(() =>
        {
            // Check if cancellation has been requested, and throw if it has
            token.ThrowIfCancellationRequested();

            // Validate the provided seed phrase to ensure it's correct and usable
            if (!SeedPhraseValidator.IsValidSeedPhrase(seedPhrase))
            {
                throw new ArgumentException("SeedPhrase is invalid."); // Throw exception if invalid seed phrase
            }

            // Restore the account from the provided valid seed phrase
            Mnemonic mnemonic = new(seedPhrase);
            PrivateKey privateKey = RadixBridgeHelper.GetPrivateKey(mnemonic);

            // Convert the restored public key to a hexadecimal string for easy representation
            string publicKey = Encoders.Hex.EncodeData(privateKey.PublicKeyBytes());

            // Return a tuple containing the public key and private key
            return (publicKey, privateKey.RawHex());
        }, token);
    }

    #endregion
}