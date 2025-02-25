using TransactionBuilder = RadixEngineToolkit.TransactionBuilder;

namespace RadixBridge;

/// <summary>
/// Represents a bridge for interacting with the Radix network.
/// Provides functionality to manage accounts, check balances, 
/// and execute transactions (withdraw/deposit) with detailed logging.
/// </summary>
public sealed class RadixBridge : IRadixBridge
{
    private readonly RadixTechnicalAccountBridgeOptions _options; // Options for configuring the bridge
    private readonly HttpClient _httpClient; // HTTP client for making requests to the Radix API
    private readonly string _network; // Network name (MainNet or StokeNet)
    private readonly Address _xrdAddress; // Address associated with the XRD resource
    private readonly ILogger<RadixBridge> _logger; // Logger

    /// <summary>
    /// Initializes a new instance of the RadixBridge class.
    /// Sets the network and XRD address based on the provided options.
    /// </summary>
    public RadixBridge(RadixTechnicalAccountBridgeOptions options, HttpClient httpClient, ILogger<RadixBridge> logger)
    {
        _logger = logger;
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        _logger.LogInformation("Initializing RadixBridge. Checking NetworkId at {Time}", DateTimeOffset.UtcNow);
        if (options.NetworkId == (byte)NetworkType.Main)
        {
            _network = RadixBridgeHelper.MainNet;
            _xrdAddress = new Address(RadixBridgeHelper.MainNetXrdAddress);
            _logger.LogInformation("Network set to MainNet. XRD Address: {Address}", _xrdAddress.AddressString());
        }
        else
        {
            _network = RadixBridgeHelper.StokeNet;
            _xrdAddress = new Address(RadixBridgeHelper.StokeNetXrdAddress);
            _logger.LogInformation("Network set to StokeNet. XRD Address: {Address}", _xrdAddress.AddressString());
        }
    }

    /// <summary>
    /// Retrieves the balance of an account.
    /// </summary>
    public async Task<Result<decimal>> GetAccountBalanceAsync(string accountAddress, CancellationToken token = default)
    {
        _logger.LogInformation("Starting GetAccountBalanceAsync for account {Account} at {Time}", accountAddress,
            DateTimeOffset.UtcNow);
        token.ThrowIfCancellationRequested();

        var data = new
        {
            network = _network,
            account_address = accountAddress, // Account address to check balance
            resource_address = _xrdAddress.AddressString() // XRD resource address
        };

        // Send request to Radix API to retrieve fungible resource balance
        _logger.LogInformation("Sending request to Radix API for account balance.");
        AccountFungibleResourceBalanceDto? result =
            await RadixHttpClientHelper.PostAsync<object, AccountFungibleResourceBalanceDto>(
                _httpClient,
                $"{_options.HostUri}/core/lts/state/account-fungible-resource-balance",
                data,
                token
            );

        _logger.LogInformation("Received response from Radix API for account balance at {Time}", DateTimeOffset.UtcNow);
        _logger.LogInformation("Finished GetAccountBalanceAsync for account {Account} at {Time}", accountAddress,
            DateTimeOffset.UtcNow);

        return result != null
            ? Result<decimal>.Success(decimal.Parse(result.FungibleResourceBalance.Amount))
            : Result<decimal>.Success();
    }

    /// <summary>
    /// Creates a new Radix account with a randomly generated seed phrase.
    /// </summary>
    public Result<(PublicKey PublicKey, PrivateKey PrivateKey, string SeedPhrase)> CreateAccountAsync(
        CancellationToken token = default)
    {
        _logger.LogInformation("Starting CreateAccountAsync at {Time}", DateTimeOffset.UtcNow);
        token.ThrowIfCancellationRequested();

        // Generate a new mnemonic (seed phrase) using an English word list
        _logger.LogInformation("Generating mnemonic for new account.");
        Mnemonic mnemonic = new(Wordlist.English, WordCount.TwentyFour);
        // Derive the private key from the mnemonic
        _logger.LogInformation("Deriving private key from mnemonic.");
        PrivateKey privateKey = RadixBridgeHelper.GetPrivateKey(mnemonic);

        _logger.LogInformation("Finished CreateAccountAsync at {Time}", DateTimeOffset.UtcNow);
        return Result<(PublicKey PublicKey, PrivateKey PrivateKey, string SeedPhrase)>
            .Success((privateKey.PublicKey(), privateKey, mnemonic.ToString()));
    }

    /// <summary>
    /// Restores an account using a seed phrase.
    /// </summary>
    public Result<(PublicKey PublicKey, PrivateKey PrivateKey)> RestoreAccountAsync(string seedPhrase,
        CancellationToken token = default)
    {
        _logger.LogInformation("Starting RestoreAccountAsync at {Time}", DateTimeOffset.UtcNow);
        token.ThrowIfCancellationRequested();

        if (!SeedPhraseValidator.IsValidSeedPhrase(seedPhrase))
        {
            _logger.LogWarning("Invalid seed phrase format provided.");
            return Result<(PublicKey PublicKey, PrivateKey PrivateKey)>
                .Failure(ResultPatternError.BadRequest("SeedPhrase format is invalid."));
        }

        _logger.LogInformation("Creating mnemonic from seed phrase.");
        Mnemonic mnemonic = new(seedPhrase);
        _logger.LogInformation("Deriving private key from mnemonic.");
        PrivateKey privateKey = RadixBridgeHelper.GetPrivateKey(mnemonic);

        _logger.LogInformation("Finished RestoreAccountAsync at {Time}", DateTimeOffset.UtcNow);
        return Result<(PublicKey PublicKey, PrivateKey PrivateKey)>
            .Success((privateKey.PublicKey(), privateKey));
    }

    /// <summary>
    /// Retrieves the address of an account or identity based on the public key.
    /// </summary>
    public Result<string> GetAddressAsync(PublicKey publicKey, AddressType addressType, NetworkType networkType,
        CancellationToken token = default)
    {
        _logger.LogInformation("Starting GetAddressAsync at {Time}", DateTimeOffset.UtcNow);
        token.ThrowIfCancellationRequested();

        byte network = (byte)networkType;
        _logger.LogInformation("Finished GetAddressAsync at {Time}", DateTimeOffset.UtcNow);

        return addressType switch
        {
            AddressType.Account => Result<string>.Success(Address.VirtualAccountAddressFromPublicKey(publicKey, network)
                .AddressString()),
            AddressType.Identity => Result<string>.Success(Address
                .VirtualIdentityAddressFromPublicKey(publicKey, network).AddressString()),
            _ => Result<string>.Failure(ResultPatternError.BadRequest("Invalid address type."))
        };
    }

    /// <summary>
    /// Withdraws a specified amount from an account.
    /// </summary>
    public async Task<Result<TransactionResponse>> WithdrawAsync(decimal amount, string senderAccountAddress,
        string senderPrivateKey)
    {
        _logger.LogInformation("Initiating WithdrawAsync for account {Account} at {Time}", senderAccountAddress,
            DateTimeOffset.UtcNow);
        if (senderAccountAddress == _options.AccountAddress)
        {
            _logger.LogWarning("Withdrawal attempted from tech account. Operation not allowed.");
            return Result<TransactionResponse>.Failure(
                ResultPatternError.BadRequest("Transaction from tech account to the same account is not allowed."));
        }

        return await ExecuteTransactionAsync(amount, senderAccountAddress, senderPrivateKey, true);
    }

    /// <summary>
    /// Deposits a specified amount to an account.
    /// </summary>
    public async Task<Result<TransactionResponse>> DepositAsync(decimal amount, string receiverAccountAddress)
    {
        _logger.LogInformation("Initiating DepositAsync for account {Account} at {Time}", receiverAccountAddress,
            DateTimeOffset.UtcNow);
        if (receiverAccountAddress == _options.AccountAddress)
        {
            _logger.LogWarning("Deposit attempted to tech account. Operation not allowed.");
            return Result<TransactionResponse>.Failure(
                ResultPatternError.BadRequest("Transaction from tech account to the same account is not allowed."));
        }

        return await ExecuteTransactionAsync(amount, receiverAccountAddress, _options.PrivateKey, false);
    }

    /// <summary>
    /// Executes a transaction (withdrawal or deposit) by building and submitting the transaction manifest.
    /// Detailed logging is provided at each step.
    /// </summary>
    private async Task<Result<TransactionResponse>> ExecuteTransactionAsync(decimal amount, string accountAddress,
        string privateKey, bool isWithdraw)
    {
        try
        {
            _logger.LogInformation("Starting ExecuteTransactionAsync at {Time}", DateTimeOffset.UtcNow);

            // Create sender's private key and derive sender address
            _logger.LogInformation("Decoding private key and creating sender's PrivateKey instance.");
            using PrivateKey senderPrivateKey = new PrivateKey(Encoders.Hex.DecodeData(privateKey), Curve.ED25519);
            _logger.LogInformation("Deriving sender's address from private key.");
            using Address sender =
                Address.VirtualAccountAddressFromPublicKey(senderPrivateKey.PublicKey(), _options.NetworkId);

            // Determine receiver's address based on transaction type
            _logger.LogInformation("Setting receiver's address.");
            Address receiver = new Address(accountAddress);
            if (isWithdraw)
            {
                _logger.LogInformation("Withdrawal operation: overriding receiver with tech account address.");
                receiver = new Address(_options.AccountAddress);
            }

            // Retrieve account balance
            _logger.LogInformation("Retrieving account balance for sender: {SenderAddress}", sender.AddressString());
            Result<decimal> balanceResult = await GetAccountBalanceAsync(sender.AddressString());
            if (!balanceResult.IsSuccess)
            {
                _logger.LogError("Failed to retrieve balance: {Error}", balanceResult.Error.Message);
                return Result<TransactionResponse>.Failure(balanceResult.Error);
            }

            // Validate sufficient balance
            if (amount >= balanceResult.Value)
            {
                _logger.LogWarning(
                    "Insufficient funds: Requested amount {Amount} is greater than or equal to balance {Balance}",
                    amount, balanceResult.Value);
                return Result<TransactionResponse>.Failure(
                    ResultPatternError.BadRequest("Amount is too small in tech account to be included."),
                    new TransactionResponse("", null, false, "Invalid amount",
                        BridgeTransactionStatus.InsufficientFunds));
            }

            // Round the amount to 18 decimal places
            _logger.LogInformation("Rounding amount {Amount} to 18 decimal places.", amount);
            decimal roundedAmount = Math.Round(amount, 18, MidpointRounding.ToZero);

            // Build the transaction manifest with detailed logging
            _logger.LogInformation("Building transaction manifest.");
            using TransactionManifest manifest = new ManifestBuilder()
                .AccountLockFeeAndWithdraw(sender, new("100"), _xrdAddress, new($"{roundedAmount}"))
                .TakeFromWorktop(_xrdAddress, new($"{roundedAmount}"), new("xrdBucket"))
                .AccountTryDepositOrAbort(receiver, new("xrdBucket"), null)
                .Build(_options.NetworkId);
            _logger.LogInformation("Transaction manifest built. Validating manifest...");
            manifest.StaticallyValidate();
            _logger.LogInformation("Manifest validated successfully.");

            // Retrieve current epoch for transaction header
            _logger.LogInformation("Retrieving current epoch from construction metadata.");
            ulong currentEpoch = (await _httpClient.GetConstructionMetadata(_options))?.CurrentEpoch ?? 0;
            _logger.LogInformation("Current epoch is {Epoch}", currentEpoch);

            // Build and notarize the transaction
            _logger.LogInformation("Building transaction header and notarizing transaction.");
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
                .Manifest(manifest)
                .Message(new Message.None())
                .NotarizeWithPrivateKey(senderPrivateKey);
            _logger.LogInformation("Transaction compiled successfully.");

            // Prepare data payload for transaction submission
            _logger.LogInformation("Preparing data payload for transaction submission.");
            var data = new
            {
                network = _network,
                notarized_transaction_hex = Encoders.Hex.EncodeData(transaction.Compile()),
                force_recalculate = true
            };

            // Submit transaction to the Radix network
            _logger.LogInformation("Submitting transaction to Radix network.");
            TransactionSubmitResponse? response =
                await RadixHttpClientHelper.PostAsync<object, TransactionSubmitResponse>(
                    _httpClient,
                    $"{_options.HostUri}/core/lts/transaction/submit",
                    data
                );
            _logger.LogInformation("Transaction submission completed.");

            // Return transaction response
            _logger.LogInformation("Returning successful transaction response.");
            return Result<TransactionResponse>.Success(new TransactionResponse(
                transaction.IntentHash().AsStr(),
                response?.Duplicate.ToString(),
                response != null,
                response == null ? "Transaction failed" : null,
                response != null ? BridgeTransactionStatus.Completed : BridgeTransactionStatus.Canceled
            ));
        }
        catch (Exception e)
        {
            _logger.LogError("Error during ExecuteTransactionAsync: {Message}", e.Message);
            return Result<TransactionResponse>.Failure(ResultPatternError.InternalServerError(e.Message));
        }
        finally
        {
            _logger.LogInformation("Finishing ExecuteTransactionAsync at {Time}", DateTimeOffset.UtcNow);
        }
    }

    /// <summary>
    /// Retrieves the status of a submitted transaction.
    /// </summary>
    public async Task<Result<BridgeTransactionStatus>> GetTransactionStatusAsync(string transactionHash,
        CancellationToken token = default)
    {
        _logger.LogInformation("Starting GetTransactionStatusAsync at {Time}", DateTimeOffset.UtcNow);
        token.ThrowIfCancellationRequested();

        var data = new
        {
            network = _network,
            intent_hash = transactionHash
        };

        _logger.LogInformation("Sending request to Radix for transaction status.");
        TransactionStatusResponse? response = await RadixHttpClientHelper.PostAsync<object, TransactionStatusResponse>(
            _httpClient,
            $"{_options.HostUri}/core/transaction/status",
            data,
            token
        );
        _logger.LogInformation("Received transaction status from Radix at {Time}", DateTimeOffset.UtcNow);

        if (response is null)
        {
            _logger.LogError("Radix GetTransactionStatus method returned null.");
            return Result<BridgeTransactionStatus>.Failure(
                ResultPatternError.InternalServerError("Error in Radix GetTransactionStatus method"));
        }

        _logger.LogInformation("Interpreting transaction status: {Status}", response.IntentStatus);
        return response.IntentStatus switch
        {
            RadixTransactionStatus.CommittedSuccess => Result<BridgeTransactionStatus>.Success(BridgeTransactionStatus
                .Completed),
            RadixTransactionStatus.CommittedFailure => Result<BridgeTransactionStatus>.Success(BridgeTransactionStatus
                .Canceled),
            RadixTransactionStatus.NotSeen => Result<BridgeTransactionStatus>.Success(BridgeTransactionStatus.NotFound),
            RadixTransactionStatus.InMemPool=> Result<BridgeTransactionStatus>.Success(BridgeTransactionStatus.Completed),
            RadixTransactionStatus.PermanentRejection=> Result<BridgeTransactionStatus>.Success(BridgeTransactionStatus.Canceled),
            RadixTransactionStatus.FateUncertainButLikelyRejection=> Result<BridgeTransactionStatus>.Success(BridgeTransactionStatus.Canceled),
            RadixTransactionStatus.FateUncertain=>Result<BridgeTransactionStatus>.Success(BridgeTransactionStatus.Pending),
               _ => throw new ArgumentOutOfRangeException()
        };
    }

    #region Implementation for contract IBridge

    /// <summary>
    /// Creates a new account asynchronously using a randomly generated seed phrase.
    /// </summary>
    async Task<Result<(string PublicKey, string PrivateKey, string SeedPhrase)>> IBridge.CreateAccountAsync(
        CancellationToken token)
    {
        try
        {
            _logger.LogInformation("Starting IBridge.CreateAccountAsync at {Time}", DateTimeOffset.UtcNow);
            token.ThrowIfCancellationRequested();

            _logger.LogInformation("Generating mnemonic for IBridge account creation.");
            Mnemonic mnemonic = new(Wordlist.English, WordCount.TwentyFour);
            _logger.LogInformation("Deriving private key from mnemonic for IBridge account.");
            using PrivateKey privateKey = RadixBridgeHelper.GetPrivateKey(mnemonic);
            string publicKey = Encoders.Hex.EncodeData(privateKey.PublicKeyBytes());

            _logger.LogInformation("IBridge account created successfully.");
            return Result<(string PublicKey, string PrivateKey, string SeedPhrase)>
                .Success((publicKey, privateKey.RawHex(), mnemonic.ToString()));
        }
        catch (Exception e)
        {
            _logger.LogError( "Error in IBridge.CreateAccountAsync: {Message}", e.Message);
            await Task.CompletedTask;
            return Result<(string PublicKey, string PrivateKey, string SeedPhrase)>
                .Failure(ResultPatternError.InternalServerError(e.Message));
        }
        finally
        {
            _logger.LogInformation("Finishing IBridge.CreateAccountAsync at {Time}", DateTimeOffset.UtcNow);
        }
    }

    /// <summary>
    /// Restores an account asynchronously using a provided seed phrase.
    /// </summary>
    async Task<Result<(string PublicKey, string PrivateKey)>> IBridge.RestoreAccountAsync(string seedPhrase,
        CancellationToken token)
    {
        try
        {
            _logger.LogInformation("Starting IBridge.RestoreAccountAsync at {Time}", DateTimeOffset.UtcNow);
            token.ThrowIfCancellationRequested();

            if (!SeedPhraseValidator.IsValidSeedPhrase(seedPhrase))
            {
                _logger.LogWarning("Invalid seed phrase format provided for IBridge.RestoreAccountAsync.");
                return Result<(string PublicKey, string PrivateKey)>
                    .Failure(ResultPatternError.BadRequest("SeedPhrase format is invalid."));
            }

            _logger.LogInformation("Creating mnemonic from seed phrase for account restoration.");
            Mnemonic mnemonic = new(seedPhrase);
            _logger.LogInformation("Deriving private key for account restoration.");
            PrivateKey privateKey = RadixBridgeHelper.GetPrivateKey(mnemonic);
            string publicKey = Encoders.Hex.EncodeData(privateKey.PublicKeyBytes());

            _logger.LogInformation("IBridge account restored successfully.");
            return Result<(string PublicKey, string PrivateKey)>
                .Success((publicKey, privateKey.RawHex()));
        }
        catch (Exception e)
        {
            _logger.LogError( "Error in IBridge.RestoreAccountAsync: {Message}", e.Message);
            await Task.CompletedTask;
            return Result<(string PublicKey, string PrivateKey)>
                .Failure(ResultPatternError.InternalServerError(e.Message));
        }
        finally
        {
            _logger.LogInformation("Finishing IBridge.RestoreAccountAsync at {Time}", DateTimeOffset.UtcNow);
        }
    }

    #endregion
}