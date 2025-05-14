using TransactionResponse = Application.DTOs.NftPurchase.Responses.TransactionResponse;

namespace Infrastructure.ImplementationContract.Nft;

public sealed class SolShiftIntegrationService(
    ILogger<SolShiftIntegrationService> logger,
    DataContext dbContext,
    IHttpContextAccessor accessor,
    IHttpClientFactory httpClientFactory) : ISolShiftIntegrationService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClientNames.SolShiftClient);

    public async Task<Result<TransactionResponse>> CreateTransactionAsync(CreateTransactionRequest request)
    {
        DateTimeOffset date = DateTimeOffset.UtcNow;
        logger.OperationStarted(nameof(CreateTransactionAsync), date);

        try
        {
           Result<TransactionResponse> res= await HttpClientHelper
               .PostAsync<CreateTransactionRequest,TransactionResponse>()
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

    public async Task<Result<TransactionResponse>> SendTransactionAsync(string transactionId)
    {
        DateTimeOffset date = DateTimeOffset.UtcNow;
        logger.OperationStarted(nameof(CreateTransactionAsync), date);

        try
        {
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
}