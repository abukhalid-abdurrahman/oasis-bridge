namespace Infrastructure.ImplementationContract;

public sealed class NetworkService(
    DataContext dbContext,
    IHttpContextAccessor accessor) : INetworkService
{
    public async Task<Result<PagedResponse<IEnumerable<GetNetworkResponse>>>> GetNetworksAsync(NetworkFilter filter,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        IQueryable<GetNetworkResponse> query = dbContext.Networks
            .AsNoTracking()
            .ApplyFilter(filter.Name, x => x.Name)
            .ApplyFilter(filter.Description, x => x.Description)
            .Include(n => n.NetworkTokens)
            .Select(n => new GetNetworkResponse(
                n.Id,
                n.Name,
                n.Description,
                n.NetworkTokens.Select(nt => nt.Symbol).ToList()
            ));

        int totalCount = await query.CountAsync(token);

        var result = PagedResponse<IEnumerable<GetNetworkResponse>>.Create(
            filter.PageSize,
            filter.PageNumber,
            totalCount,
            query.Page(filter.PageNumber, filter.PageSize));

        return Result<PagedResponse<IEnumerable<GetNetworkResponse>>>.Success(result);
    }

    public async Task<Result<GetNetworkDetailResponse>> GetNetworkDetailAsync(Guid networkId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        GetNetworkDetailResponse? network = await dbContext.Networks.AsNoTracking()
            .Where(x => x.Id == networkId)
            .Include(n => n.NetworkTokens)
            .Select(n => new GetNetworkDetailResponse(
                n.Id,
                n.Name,
                n.Description,
                n.NetworkTokens.Select(nt => nt.Symbol).ToList()
            )).FirstOrDefaultAsync(token);

        return network is not null
            ? Result<GetNetworkDetailResponse>.Success(network)
            : Result<GetNetworkDetailResponse>.Failure(ResultPatternError.NotFound("Network not found"));
    }

    public async Task<Result<CreateNetworkResponse>> CreateNetworkAsync(CreateNetworkRequest request,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        bool networkExists = await dbContext.Networks
            .AnyAsync(x => x.Name == request.Name, token);

        if (networkExists)
            return Result<CreateNetworkResponse>.Failure(
                ResultPatternError.Conflict("Network name already exists"));

        Network newNetwork = request.ToEntity(accessor);
        await dbContext.Networks.AddAsync(newNetwork, token);
        int res = await dbContext.SaveChangesAsync(token);

        return res != 0
            ? Result<CreateNetworkResponse>.Success(new CreateNetworkResponse(newNetwork.Id))
            : Result<CreateNetworkResponse>.Failure(ResultPatternError.InternalServerError("Data not saved"));
    }

    public async Task<Result<UpdateNetworkResponse>> UpdateNetworkAsync(Guid networkId, UpdateNetworkRequest request,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        Network? network = await dbContext.Networks.FirstOrDefaultAsync(x => x.Id == networkId, token);
        if (network is null)
            return Result<UpdateNetworkResponse>.Failure(ResultPatternError.NotFound("Network not found"));

        if (!string.IsNullOrEmpty(request.Name) && request.Name != network.Name)
        {
            bool nameExists = await dbContext.Networks
                .AnyAsync(x => x.Name == request.Name && x.Id != networkId, token);
            if (nameExists)
                return Result<UpdateNetworkResponse>.Failure(
                    ResultPatternError.Conflict("Network name already exists"));
        }

        if (request.Description is not null)
            network.Description = request.Description;

        network.ToEntity(accessor, request);
        int res = await dbContext.SaveChangesAsync(token);

        return res != 0
            ? Result<UpdateNetworkResponse>.Success(new(networkId))
            : Result<UpdateNetworkResponse>.Failure(ResultPatternError.InternalServerError("Couldn't update network"));
    }

    public async Task<Result<DeleteNetworkResponse>> DeleteNetworkAsync(Guid networkId,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        Network? network = await dbContext.Networks.FirstOrDefaultAsync(x => x.Id == networkId, token);

        if (network is null)
            return Result<DeleteNetworkResponse>.Failure(ResultPatternError.NotFound("Network not found"));

        network.ToEntity(accessor);
        int res = await dbContext.SaveChangesAsync(token);

        return res != 0
            ? Result<DeleteNetworkResponse>.Success(new(networkId))
            : Result<DeleteNetworkResponse>.Failure(ResultPatternError.InternalServerError("Couldn't delete network"));
    }
}