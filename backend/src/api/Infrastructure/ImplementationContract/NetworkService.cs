namespace Infrastructure.ImplementationContract;

public sealed class NetworkService(
    DataContext dbContext,
    IHttpContextAccessor accessor,
    ILogger<NetworkService> logger) : INetworkService
{
    /// <summary>
    /// Retrieves a paged list of networks based on the provided filter.
    /// </summary>
    public async Task<Result<PagedResponse<IEnumerable<GetNetworkResponse>>>> GetNetworksAsync(NetworkFilter filter,
        CancellationToken token = default)
    {
        logger.LogInformation("Starting GetNetworksAsync at {Time}", DateTimeOffset.UtcNow);
        token.ThrowIfCancellationRequested();

        logger.LogInformation("Building query for networks with filter - Name: {Name}, Description: {Description}",
            filter.Name, filter.Description);
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

        logger.LogInformation("Counting total networks matching filter criteria.");
        int totalCount = await query.CountAsync(token);
        logger.LogInformation("Total networks count: {TotalCount}", totalCount);

        logger.LogInformation("Applying pagination: PageNumber = {PageNumber}, PageSize = {PageSize}",
            filter.PageNumber, filter.PageSize);
        PagedResponse<IEnumerable<GetNetworkResponse>> pagedResult = PagedResponse<IEnumerable<GetNetworkResponse>>.Create(
            filter.PageSize,
            filter.PageNumber,
            totalCount,
            query.Page(filter.PageNumber, filter.PageSize));

        logger.LogInformation("Finishing GetNetworksAsync at {Time}", DateTimeOffset.UtcNow);
        return Result<PagedResponse<IEnumerable<GetNetworkResponse>>>.Success(pagedResult);
    }

    /// <summary>
    /// Retrieves detailed information about a specific network.
    /// </summary>
    public async Task<Result<GetNetworkDetailResponse>> GetNetworkDetailAsync(Guid networkId, CancellationToken token)
    {
        logger.LogInformation("Starting GetNetworkDetailAsync for NetworkId: {NetworkId} at {Time}", networkId,
            DateTimeOffset.UtcNow);
        token.ThrowIfCancellationRequested();

        GetNetworkDetailResponse? network = await dbContext.Networks
            .AsNoTracking()
            .Where(x => x.Id == networkId)
            .Include(n => n.NetworkTokens)
            .Select(n => new GetNetworkDetailResponse(
                n.Id,
                n.Name,
                n.Description,
                n.NetworkTokens.Select(nt => nt.Symbol).ToList()
            ))
            .FirstOrDefaultAsync(token);

        if (network is not null)
        {
            logger.LogInformation("Network detail retrieved successfully for NetworkId: {NetworkId}", networkId);
            return Result<GetNetworkDetailResponse>.Success(network);
        }
        else
        {
            logger.LogWarning("Network not found for NetworkId: {NetworkId}", networkId);
            return Result<GetNetworkDetailResponse>.Failure(ResultPatternError.NotFound("Network not found"));
        }
    }

    /// <summary>
    /// Creates a new network based on the provided request.
    /// </summary>
    public async Task<Result<CreateNetworkResponse>> CreateNetworkAsync(CreateNetworkRequest request,
        CancellationToken token)
    {
        logger.LogInformation("Starting CreateNetworkAsync at {Time}", DateTimeOffset.UtcNow);
        token.ThrowIfCancellationRequested();

        logger.LogInformation("Checking if network with name {Name} already exists.", request.Name);
        bool networkExists = await dbContext.Networks.AnyAsync(x => x.Name == request.Name, token);
        if (networkExists)
        {
            logger.LogWarning("Network with name {Name} already exists.", request.Name);
            return Result<CreateNetworkResponse>.Failure(ResultPatternError.Conflict("Network name already exists"));
        }

        logger.LogInformation("Mapping CreateNetworkRequest to Network entity.");
        Network newNetwork = request.ToEntity(accessor);
        logger.LogInformation("Adding new network to the database.");
        await dbContext.Networks.AddAsync(newNetwork, token);
        int res = await dbContext.SaveChangesAsync(token);

        if (res != 0)
        {
            logger.LogInformation("Network created successfully with ID: {NetworkId}", newNetwork.Id);
            logger.LogInformation("Finishing CreateNetworkAsync at {Time}", DateTimeOffset.UtcNow);
            return Result<CreateNetworkResponse>.Success(new CreateNetworkResponse(newNetwork.Id));
        }
        else
        {
            logger.LogError("Failed to save new network to the database.");
            return Result<CreateNetworkResponse>.Failure(ResultPatternError.InternalServerError("Data not saved"));
        }
    }

    /// <summary>
    /// Updates an existing network with new details.
    /// </summary>
    public async Task<Result<UpdateNetworkResponse>> UpdateNetworkAsync(Guid networkId, UpdateNetworkRequest request,
        CancellationToken token = default)
    {
        logger.LogInformation("Starting UpdateNetworkAsync for NetworkId: {NetworkId} at {Time}", networkId,
            DateTimeOffset.UtcNow);
        token.ThrowIfCancellationRequested();

        logger.LogInformation("Retrieving network with ID: {NetworkId}", networkId);
        Network? network = await dbContext.Networks.FirstOrDefaultAsync(x => x.Id == networkId, token);
        if (network is null)
        {
            logger.LogWarning("Network with ID: {NetworkId} not found.", networkId);
            return Result<UpdateNetworkResponse>.Failure(ResultPatternError.NotFound("Network not found"));
        }

        if (!string.IsNullOrEmpty(request.Name) && request.Name != network.Name)
        {
            logger.LogInformation("Checking if new network name {Name} is already in use.", request.Name);
            bool nameExists =
                await dbContext.Networks.AnyAsync(x => x.Name == request.Name && x.Id != networkId, token);
            if (nameExists)
            {
                logger.LogWarning("Network name {Name} already exists.", request.Name);
                return Result<UpdateNetworkResponse>.Failure(
                    ResultPatternError.Conflict("Network name already exists"));
            }
        }

        logger.LogInformation("Updating network properties.");
        if (request.Description is not null)
            network.Description = request.Description;

        // Assuming ToEntity updates the network properties using accessor and request values.
        network.ToEntity(accessor, request);
        int res = await dbContext.SaveChangesAsync(token);
        if (res != 0)
        {
            logger.LogInformation("Network updated successfully for NetworkId: {NetworkId}", networkId);
            return Result<UpdateNetworkResponse>.Success(new UpdateNetworkResponse(networkId));
        }
        else
        {
            logger.LogError("Failed to update network for NetworkId: {NetworkId}", networkId);
            return Result<UpdateNetworkResponse>.Failure(
                ResultPatternError.InternalServerError("Couldn't update network"));
        }
    }

    /// <summary>
    /// Deletes a network by its ID.
    /// </summary>
    public async Task<Result<DeleteNetworkResponse>> DeleteNetworkAsync(Guid networkId,
        CancellationToken token = default)
    {
        logger.LogInformation("Starting DeleteNetworkAsync for NetworkId: {NetworkId} at {Time}", networkId,
            DateTimeOffset.UtcNow);
        token.ThrowIfCancellationRequested();

        logger.LogInformation("Retrieving network with ID: {NetworkId}", networkId);
        Network? network = await dbContext.Networks.FirstOrDefaultAsync(x => x.Id == networkId, token);
        if (network is null)
        {
            logger.LogWarning("Network with ID: {NetworkId} not found.", networkId);
            return Result<DeleteNetworkResponse>.Failure(ResultPatternError.NotFound("Network not found"));
        }

        logger.LogInformation("Mapping network for deletion using accessor.");
        network.ToEntity(accessor);
        int res = await dbContext.SaveChangesAsync(token);
        if (res != 0)
        {
            logger.LogInformation("Network with ID: {NetworkId} deleted successfully.", networkId);
            return Result<DeleteNetworkResponse>.Success(new DeleteNetworkResponse(networkId));
        }
        else
        {
            logger.LogError("Failed to delete network with ID: {NetworkId}", networkId);
            return Result<DeleteNetworkResponse>.Failure(
                ResultPatternError.InternalServerError("Couldn't delete network"));
        }
    }
}