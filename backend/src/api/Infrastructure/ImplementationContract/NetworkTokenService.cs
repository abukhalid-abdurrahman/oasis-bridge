namespace Infrastructure.ImplementationContract;

public sealed class NetworkTokenService(
    DataContext dbContext,
    IHttpContextAccessor accessor) : INetworkTokenService
{
    public async Task<Result<PagedResponse<IEnumerable<GetNetworkTokenResponse>>>> GetNetworkTokensAsync(
        NetworkTokenFilter filter, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        IQueryable<GetNetworkTokenResponse> query = dbContext.NetworkTokens
             .AsNoTracking()
            .ApplyFilter(filter.Symbol, x => x.Symbol)
            .ApplyFilter(filter.Description, x => x.Description)
            .Select(x => x.ToRead());

        int totalCount = await query.CountAsync(token);

        var result = PagedResponse<IEnumerable<GetNetworkTokenResponse>>.Create(
            filter.PageSize,
            filter.PageNumber,
            totalCount,
            query.Page(filter.PageNumber, filter.PageSize));

        return Result<PagedResponse<IEnumerable<GetNetworkTokenResponse>>>.Success(result);
    }

    public async Task<Result<GetNetworkTokenDetailResponse>> GetNetworkTokenDetailAsync(Guid networkTokenId,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        GetNetworkTokenDetailResponse? networkToken = await dbContext.NetworkTokens
            .AsNoTracking()
            .Where(x => x.Id == networkTokenId)
            .Select(x => x.ToReadDetail())
            .FirstOrDefaultAsync(token);

        return networkToken is not null
            ? Result<GetNetworkTokenDetailResponse>.Success(networkToken)
            : Result<GetNetworkTokenDetailResponse>.Failure(ResultPatternError.NotFound("Network Token not found"));
    }

    public async Task<Result<CreateNetworkTokenResponse>> CreateNetworkTokenAsync(CreateNetworkTokenRequest request,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        bool tokenExists = await dbContext.NetworkTokens
            .AnyAsync(x => x.Symbol == request.Symbol && x.NetworkId == request.NetworkId, token);

        if (tokenExists)
            return Result<CreateNetworkTokenResponse>.Failure(
                ResultPatternError.Conflict("Network Token already exists"));

        NetworkToken newToken = request.ToEntity(accessor);
        await dbContext.NetworkTokens.AddAsync(newToken, token);
        int res = await dbContext.SaveChangesAsync(token);

        return res != 0
            ? Result<CreateNetworkTokenResponse>.Success(new CreateNetworkTokenResponse(newToken.Id))
            : Result<CreateNetworkTokenResponse>.Failure(ResultPatternError.InternalServerError("Data not saved"));
    }

    public async Task<Result<UpdateNetworkTokenResponse>> UpdateNetworkTokenAsync(Guid networkTokenId,
        UpdateNetworkTokenRequest request, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        NetworkToken? networkToken =
            await dbContext.NetworkTokens.FirstOrDefaultAsync(x => x.Id == networkTokenId, token);
        if (networkToken is null)
            return Result<UpdateNetworkTokenResponse>.Failure(ResultPatternError.NotFound("Network Token not found"));

        if (!string.IsNullOrEmpty(request.Symbol) && request.Symbol != networkToken.Symbol)
        {
            bool symbolExists = await dbContext.NetworkTokens
                .AnyAsync(
                    x => x.Symbol == request.Symbol && x.NetworkId == networkToken.NetworkId && x.Id != networkTokenId,
                    token);
            if (symbolExists)
                return Result<UpdateNetworkTokenResponse>.Failure(
                    ResultPatternError.Conflict("Network Token symbol already exists"));
        }


        networkToken.ToEntity(accessor, request);
        int res = await dbContext.SaveChangesAsync(token);

        return res != 0
            ? Result<UpdateNetworkTokenResponse>.Success(new(networkTokenId))
            : Result<UpdateNetworkTokenResponse>.Failure(
                ResultPatternError.InternalServerError("Couldn't update Network Token"));
    }

    public async Task<Result<DeleteNetworkTokenResponse>> DeleteNetworkTokenAsync(Guid networkTokenId,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        NetworkToken? networkToken =
            await dbContext.NetworkTokens.FirstOrDefaultAsync(x => x.Id == networkTokenId, token);

        if (networkToken is null)
            return Result<DeleteNetworkTokenResponse>.Failure(ResultPatternError.NotFound("Network Token not found"));

        networkToken.ToEntity(accessor);
        int res = await dbContext.SaveChangesAsync(token);

        return res != 0
            ? Result<DeleteNetworkTokenResponse>.Success(new(networkTokenId))
            : Result<DeleteNetworkTokenResponse>.Failure(
                ResultPatternError.InternalServerError("Couldn't delete Network Token"));
    }
}