using API.Extensions;

namespace API.Controllers;

[Route("/api/test")]
public class TestController(
    ISolanaBridge solanaBridge,
    IRadixBridge radixBridge,
    IConfiguration configuration,
    HttpClient httpClient,
    RadixTechnicalAccountBridgeOptions options)
    : BaseController
{
    [HttpGet("exchange-rate")]
    public IActionResult GetExchangeRate([FromQuery] ExchangeRateRequest request)
    {
        decimal exchangeRate = Helper.GetExchangeRate(request.From, request.To);
        decimal convertedAmount = request.Amount * exchangeRate;

        return Ok(new
        {
            request.From,
            request.To,
            exchangeRate,
            request.Amount,
            convertedAmount
        });
    }

    [HttpGet("virtual-account")]
    public IActionResult GetVirtualAccount([FromQuery] VirtualAccountRequest request)
    {
        if (request is { From: Helper.Sol, To: Helper.Xrd })
            return Ok(Helper.GetRandomString(AccountData.GetSolanaAccounts().GetSolanaAddresses()));

        if (request is { From: Helper.Xrd, To: Helper.Sol })
            return Ok(Helper.GetRandomString(AccountData.GetRadixAccounts().GetRadixAddresses()));

        return BadRequest("Invalid creating virtual-account address");
    }

    [HttpGet("radix-tech-account")]
    public IActionResult GetRadixTechAccount()
        => Ok(configuration["RadixTechnicalAccountBridgeOptions:AccountAddress"]);

    [HttpGet("solana-tech-account")]
    public IActionResult GetSolanaTechAccount()
        => Ok(configuration["SolanaTechnicalAccountBridgeOptions:PublicKey"]);

    [HttpGet("get-crypto")]
    public IActionResult GetCrypto()
        => Ok(new List<string>() { Helper.Xrd, Helper.Sol });

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalanceAsync([FromQuery] string address)
    {
        decimal solanaRes = await solanaBridge.GetAccountBalanceAsync(address);
        if (solanaRes != 0) return Ok(solanaRes);
        decimal radixRes = await radixBridge.GetAccountBalanceAsync(address);
        if (radixRes != 0) return Ok(radixRes);
        return Ok(0);
    }

    [HttpGet("transaction-status")]
    public async Task<IActionResult> GetTransactionStatusAsync([FromQuery] string transactionId)
    {
        BridgeTransactionStatus radixRes = await radixBridge.GetTransactionStatusAsync(transactionId);
        if (radixRes == BridgeTransactionStatus.Succeed)
            return Ok(BridgeTransactionStatus.Succeed.ToString());

        BridgeTransactionStatus solanaRes = await solanaBridge.GetTransactionStatusAsync(transactionId);
        if (solanaRes == BridgeTransactionStatus.Succeed)
            return Ok(BridgeTransactionStatus.Succeed.ToString());

        if (solanaRes == BridgeTransactionStatus.NotFound && radixRes == BridgeTransactionStatus.NotFound)
            return NotFound();

        if (solanaRes == BridgeTransactionStatus.Failed || radixRes == BridgeTransactionStatus.Failed)
            return BadRequest(BridgeTransactionStatus.Failed.ToString());

        return BadRequest("Unexpected status");
    }

    [HttpPost("swap")]
    public async Task<IActionResult> SwapAsync([FromBody] SwapRequest request)
    {
        decimal rate = Helper.GetExchangeRate(request.From, request.To);
        decimal convertedAmount = request.Amount * rate;

        if (request is { From: Helper.Xrd, To: Helper.Sol })
        {
            RadixAccount? sender =
                AccountData.GetRadixAccounts().FirstOrDefault(x =>
                    x.AccountAddress == request.SenderAccountAddress || x.PublicKey == request.SenderAccountAddress);
            if (sender is null) return NotFound($"Sender account: {request.SenderAccountAddress} not found! ");

            TransactionResponse radixTrRs =
                await radixBridge.WithdrawAsync(request.Amount, sender.AccountAddress, sender.PrivateKey);
            if (!radixTrRs.Success) throw new Exception("Error sending transaction");

            TransactionResponse solanaTrRs =
                await solanaBridge.DepositAsync(convertedAmount, request.ReceiverAccountAddress);
            if (!solanaTrRs.Success)
            {
                TransactionResponse radixAbortTrRs =
                    await radixBridge.DepositAsync(request.Amount, sender.AccountAddress);
                if (!radixAbortTrRs.Success) throw new Exception("Error sending transaction");

                return BadRequest("Couldn't' make transaction,and abort transaction");
            }

            return Ok(solanaTrRs);
        }
        else if (request is { From: Helper.Sol, To: Helper.Xrd })
        {
            SolanaAccount? sender =
                AccountData.GetSolanaAccounts().FirstOrDefault(x =>
                    x.PublicKey == request.SenderAccountAddress);
            if (sender is null) return NotFound($"Sender account: {request.SenderAccountAddress} not found! ");

            TransactionResponse solanaTrRs =
                await solanaBridge.WithdrawAsync(request.Amount, sender.PublicKey, sender.PrivateKey);
            if (!solanaTrRs.Success) throw new Exception("Error sending transaction");

            TransactionResponse radixTrRs =
                await radixBridge.DepositAsync(convertedAmount, request.ReceiverAccountAddress);
            if (!radixTrRs.Success)
            {
                TransactionResponse solanaAbortTrRs =
                    await solanaBridge.DepositAsync(request.Amount, sender.PublicKey);
                if (!solanaAbortTrRs.Success) throw new Exception("Error sending transaction");

                return BadRequest("Couldn't' make transaction,and abort transaction");
            }

            return Ok(radixTrRs);
        }

        return BadRequest("Couldn't' make transaction");
    }

    [HttpPost("faucet-solana")]
    public async Task<IActionResult> FaucetSolana([FromBody] string address)
    {
        if (string.IsNullOrEmpty(address))
            return BadRequest("Address cannot be empty");

        var requestBody = new
        {
            jsonrpc = "2.0",
            id = 1,
            method = "requestAirdrop",
            @params = new object[]
            {
                address,
                1000000000
            }
        };

        string json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient.PostAsync("https://api.devnet.solana.com", content);
        string result = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
            return Ok(result);

        return BadRequest(result);
    }


    [HttpPost("faucet-radix")]
    public async Task<IActionResult> FaucetRadix([FromBody] string address)
    {
        byte networkId = 0x02;
        if (string.IsNullOrEmpty(address))
            return BadRequest("Address cannot be empty");

        PrivateKey privateKey =
            new(Encoders.Hex.DecodeData(AccountData.GetRadixAccounts()
                .Where(x => x.AccountAddress == address)
                .Select(x => x.PrivateKey)
                .FirstOrDefault()), Curve.ED25519);

        TransactionManifest manifest = new ManifestBuilder()
            .FaucetLockFee()
            .FaucetFreeXrd()
            .AccountDepositEntireWorktop(new(address))
            .Build(networkId);

        manifest.StaticallyValidate();
        ulong currentEpoch = (await httpClient.GetConstructionMetadata(options))?.CurrentEpoch ?? 0;

        using NotarizedTransaction transaction =
            new TransactionBuilder()
                .Header(
                    new TransactionHeader(
                        networkId: networkId,
                        startEpochInclusive: currentEpoch,
                        endEpochExclusive: currentEpoch + 50,
                        nonce: RadixBridgeHelper.RandomNonce(),
                        notaryPublicKey: privateKey.PublicKey(),
                        notaryIsSignatory: true,
                        tipPercentage: 0
                    )
                )
                .Manifest(manifest)
                .Message(new Message.None())
                .NotarizeWithPrivateKey(privateKey);


        var data = new
        {
            network = "stokenet",
            notarized_transaction_hex = Encoders.Hex.EncodeData(transaction.Compile()),
            force_recalculate = true
        };
        string json = JsonConvert.SerializeObject(data);

        using StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        using HttpResponseMessage response =
            await httpClient.PostAsync("https://stokenet-core.radix.live/core/lts/transaction/submit", content);

        string stringRes = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
            return Ok(transaction.IntentHash().AsStr());

        return BadRequest(stringRes);
    }
}

file static class Helper
{
    public const string Xrd = "XRD";
    public const string Sol = "SOL";

    public static string GetRandomString(List<string> array)
    {
        if (array == null || array.Count == 0)
            throw new ArgumentException("Массив не должен быть пустым");

        Random rnd = new Random();
        int index = rnd.Next(array.Count);
        return array[index];
    }

    public static decimal GetExchangeRate(string from, string to)
    {
        if (from == Sol && to == Xrd) return 7540.53m;
        if (from == Xrd && to == Sol) return 0.000052m;
        return 1;
    }


    public static List<string> GetSolanaAddresses(this List<SolanaAccount> accounts)
        => accounts.Select(x => x.PublicKey).ToList();

    public static List<string> GetRadixAddresses(this List<RadixAccount> accounts)
        => accounts.Select(x => x.AccountAddress).ToList();
}

public record VirtualAccountRequest(string From, string To);

public record ExchangeRateRequest(string From, string To, decimal Amount);

public readonly record struct ExchangeRateResponse(
    string From,
    string To,
    decimal Rate,
    decimal Amount,
    decimal ConvertedAmount,
    DateTimeOffset Timestamp);

public record SwapRequest(
    string From,
    string To,
    decimal Amount,
    string SenderAccountAddress,
    string ReceiverAccountAddress);

public record SolanaAccount(
    string PublicKey,
    string PrivateKey,
    string SeedPhrase);

public record RadixAccount(
    string PublicKey,
    string PrivateKey,
    string SeedPhrase,
    string AccountAddress);