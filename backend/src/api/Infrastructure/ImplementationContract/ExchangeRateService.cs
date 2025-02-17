namespace Infrastructure.ImplementationContract;

public sealed class ExchangeRateService : IExchangeRateService
{
    public async Task<Result<PagedResponse<IEnumerable<GetExchangeRateResponse>>>> GetExchangeRatesAsync(
        ExchangeRateFilter filter, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<GetExchangeRateDetailResponse>> GetExchangeRateDetailAsync(Guid exchangeRateId,
        CancellationToken token)
    {
        throw new NotImplementedException();
    }
}