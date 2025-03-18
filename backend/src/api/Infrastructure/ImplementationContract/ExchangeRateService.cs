namespace Infrastructure.ImplementationContract;

public sealed class ExchangeRateService(
    DataContext dbContext,
    ILogger<ExchangeRateService> logger) : IExchangeRateService
{
   
    /// <summary>
    /// Retrieves a paged list of exchange rates based on the provided filter.
    /// </summary>
    public async Task<Result<PagedResponse<IEnumerable<GetExchangeRateResponse>>>> GetExchangeRatesAsync(
        ExchangeRateFilter filter, CancellationToken token = default)
    {
        logger.LogInformation("Starting GetExchangeRatesAsync at {Time}", DateTimeOffset.UtcNow);
        token.ThrowIfCancellationRequested();

        // Build the query with applied filters
        logger.LogInformation(
            "Building exchange rates query with filters: FromTokenId = {FromTokenId}, ToTokenId = {ToTokenId}",
            filter.FromTokenId, filter.ToTokenId);
        IQueryable<GetExchangeRateResponse> exchangeRatesQuery = dbContext.ExchangeRates
            .AsNoTracking()
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

        // Count the total number of results
        logger.LogInformation("Counting total number of exchange rates matching filter criteria.");
        int totalCount = await exchangeRatesQuery.CountAsync(token);
        logger.LogInformation("Total exchange rates count: {TotalCount}", totalCount);

        // Build paged response
        logger.LogInformation("Applying pagination: PageNumber = {PageNumber}, PageSize = {PageSize}",
            filter.PageNumber, filter.PageSize);
        PagedResponse<IEnumerable<GetExchangeRateResponse>> result =
            PagedResponse<IEnumerable<GetExchangeRateResponse>>.Create(
                filter.PageSize,
                filter.PageNumber,
                totalCount,
                exchangeRatesQuery.Page(filter.PageNumber, filter.PageSize));

        logger.LogInformation("Finished GetExchangeRatesAsync at {Time}", DateTimeOffset.UtcNow);
        return Result<PagedResponse<IEnumerable<GetExchangeRateResponse>>>.Success(result);
    }

    /// <summary>
    /// Retrieves detailed information about a specific exchange rate by its ID.
    /// </summary>
    public async Task<Result<GetExchangeRateDetailResponse>> GetExchangeRateDetailAsync(Guid exchangeRateId,
        CancellationToken token)
    {
        logger.LogInformation("Starting GetExchangeRateDetailAsync for ExchangeRateId: {ExchangeRateId} at {Time}",
            exchangeRateId, DateTimeOffset.UtcNow);
        token.ThrowIfCancellationRequested();

        // Retrieve exchange rate detail using a LINQ query
        GetExchangeRateDetailResponse? exchangeRate = await dbContext.ExchangeRates
            .AsNoTracking()
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

        if (exchangeRate is not null)
        {
            logger.LogInformation("Exchange rate detail retrieved successfully for ExchangeRateId: {ExchangeRateId}",
                exchangeRateId);
            return Result<GetExchangeRateDetailResponse>.Success(exchangeRate);
        }
        else
        {
            logger.LogWarning("Exchange rate detail not found for ExchangeRateId: {ExchangeRateId}", exchangeRateId);
            return Result<GetExchangeRateDetailResponse>.Failure(
                ResultPatternError.NotFound("Exchange rate not found"));
        }
    }

    /// <summary>
    /// Retrieves the most recent exchange rate details for the specified token pair.
    /// </summary>
    public async Task<Result<GetCurrentExchangeRateDetailResponse>> GetCurrentExchangeRateDetailAsync(
        ExchangeRateRequest request, CancellationToken token = default)
    {
        logger.LogInformation(
            "Starting GetCurrentExchangeRateDetailAsync for token pair {FromToken} -> {ToToken} at {Time}",
            request.FromToken, request.ToToken, DateTimeOffset.UtcNow);
        token.ThrowIfCancellationRequested();

        // Query the latest exchange rate detail for the given token pair
        GetCurrentExchangeRateDetailResponse? exchangeRate = await dbContext.ExchangeRates
            .AsNoTracking()
            .Include(x => x.FromToken)
            .Include(x => x.ToToken)
            .Where(x => x.FromToken.Symbol == request.FromToken && x.ToToken.Symbol == request.ToToken)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new GetCurrentExchangeRateDetailResponse(
                x.Id,
                x.FromToken.Symbol,
                x.ToToken.Symbol,
                x.Rate,
                x.CreatedAt,
                "Exchange rate retrieved successfully"))
            .FirstOrDefaultAsync(token);

        if (exchangeRate is not null)
        {
            logger.LogInformation(
                "Current exchange rate detail retrieved successfully for token pair {FromToken} -> {ToToken}",
                request.FromToken, request.ToToken);
            return Result<GetCurrentExchangeRateDetailResponse>.Success(exchangeRate);
        }
        else
        {
            logger.LogWarning("Current exchange rate detail not found for token pair {FromToken} -> {ToToken}",
                request.FromToken, request.ToToken);
            return Result<GetCurrentExchangeRateDetailResponse>.Failure(
                ResultPatternError.NotFound("Exchange rate not found"));
        }
    }
}