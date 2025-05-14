using TransactionResponse = Application.DTOs.NftPurchase.Responses.TransactionResponse;

namespace Infrastructure.ImplementationContract.Nft;

public sealed class SolShiftIntegrationService(
    ILogger<SolShiftIntegrationService> logger,
    IHttpClientFactory httpClientFactory) : ISolShiftIntegrationService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClientNames.SolShiftClient);

    public async Task<Result<TransactionResponse>> CreateTransactionAsync(CreateTransactionRequest request)
    {
        DateTimeOffset date = DateTimeOffset.UtcNow;
        logger.OperationStarted(nameof(CreateTransactionAsync), date);

        string url = _httpClient.BaseAddress + "shift/create-transaction";
        try
        {
            Result<TransactionResponse> response = await HttpClientHelper
                .PostAsync<CreateTransactionRequest, TransactionResponse>(_httpClient, url, request);

            logger.OperationCompleted(nameof(CreateTransactionAsync), DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow - date);
            return response.IsSuccess
                ? Result<TransactionResponse>.Success(response.Value)
                : Result<TransactionResponse>.Failure(response.Error);
        }
        catch (Exception ex)
        {
            logger.OperationException(nameof(CreateTransactionAsync), ex.Message);
            logger.OperationCompleted(nameof(CreateTransactionAsync), DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow - date);
            return Result<TransactionResponse>.Failure(
                ResultPatternError.InternalServerError(ex.Message));
        }
    }

    public async Task<Result<TransactionResponse>> SendTransactionAsync(string signedTransaction)
    {
        DateTimeOffset date = DateTimeOffset.UtcNow;
        logger.OperationStarted(nameof(SendTransactionAsync), date);
        string url = _httpClient.BaseAddress + "shift/send-transaction";

        try
        {
            Result<TransactionResponse> response = await HttpClientHelper
                .PostAsync<string, TransactionResponse>(_httpClient, url, signedTransaction);

            logger.OperationCompleted(nameof(SendTransactionAsync), DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow - date);
            return response.IsSuccess
                ? Result<TransactionResponse>.Success(response.Value)
                : Result<TransactionResponse>.Failure(response.Error);
        }
        catch (Exception ex)
        {
            logger.OperationException(nameof(SendTransactionAsync), ex.Message);
            logger.OperationCompleted(nameof(SendTransactionAsync), DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow - date);
            return Result<TransactionResponse>.Failure(
                ResultPatternError.InternalServerError(ex.Message));
        }
    }
}