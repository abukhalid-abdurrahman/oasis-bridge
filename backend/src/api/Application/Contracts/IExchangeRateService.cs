namespace Application.Contracts;

public interface IExchangeRateService
{
    Task<Result<PagedResponse<IEnumerable<GetExchangeRateResponse>>>>
        GetExchangeRatesAsync(ExchangeRateFilter filter, CancellationToken token = default);

    Task<Result<GetExchangeRateDetailResponse>>
        GetExchangeRateDetailAsync(Guid exchangeRateId, CancellationToken token = default);

    Task<Result<GetCurrentExchangeRateDetailResponse>> GetCurrentExchangeRateDetailAsync(ExchangeRateRequest request,
        CancellationToken token = default);
}