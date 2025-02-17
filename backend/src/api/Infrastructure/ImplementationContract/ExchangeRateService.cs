namespace Infrastructure.ImplementationContract;

public sealed class ExchangeRateService(
    DataContext dbContext)
    : IExchangeRateService
{
    public async Task<Result<PagedResponse<IEnumerable<GetExchangeRateResponse>>>> GetExchangeRatesAsync(
        ExchangeRateFilter filter, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        IQueryable<GetExchangeRateResponse> exchangeRatesQuery = dbContext.ExchangeRates.AsNoTracking()
            .ApplyFilter(filter.FromTokenId.ToString(), x => x.FromTokenId.ToString())
            .ApplyFilter(filter.ToTokenId.ToString(), x => x.ToTokenId.ToString())
            .Select(x => new GetExchangeRateResponse(
                x.Id,
                x.FromTokenId,
                x.FromToken.ToReadDetail(),
                x.ToTokenId,
                x.ToToken.ToReadDetail(),
                x.Rate,
                x.CreatedAt));

        int totalCount = await exchangeRatesQuery.CountAsync(token);

        PagedResponse<IEnumerable<GetExchangeRateResponse>> result =
            PagedResponse<IEnumerable<GetExchangeRateResponse>>.Create(
                filter.PageSize,
                filter.PageNumber,
                totalCount,
                exchangeRatesQuery.Page(filter.PageNumber, filter.PageSize));

        return Result<PagedResponse<IEnumerable<GetExchangeRateResponse>>>.Success(result);
    }

    public async Task<Result<GetExchangeRateDetailResponse>> GetExchangeRateDetailAsync(Guid exchangeRateId,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        GetExchangeRateDetailResponse? exchangeRate = await dbContext.ExchangeRates.AsNoTracking()
            .Where(x => x.Id == exchangeRateId)
            .Select(x => new GetExchangeRateDetailResponse(
                x.Id,
                x.FromTokenId,
                x.FromToken.ToReadDetail(),
                x.ToTokenId,
                x.ToToken.ToReadDetail(),
                x.Rate,
                x.CreatedAt))
            .FirstOrDefaultAsync(token);

        return exchangeRate is not null
            ? Result<GetExchangeRateDetailResponse>.Success(exchangeRate)
            : Result<GetExchangeRateDetailResponse>.Failure(ResultPatternError.NotFound("Exchange rate not found"));
    }
}