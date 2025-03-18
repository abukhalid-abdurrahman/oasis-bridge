namespace Infrastructure.ImplementationContract;

public sealed class NetworkTokenService(
    DataContext dbContext,
    IHttpContextAccessor accessor,
    ILogger<NetworkTokenService> logger) : INetworkTokenService
{
    /// <summary>
    /// Retrieves a paged list of network tokens based on the provided filter.
    /// </summary>
    public async Task<Result<PagedResponse<IEnumerable<GetNetworkTokenResponse>>>> GetNetworkTokensAsync(
        NetworkTokenFilter filter, CancellationToken token = default)
    {
        logger.LogInformation("Starting GetNetworkTokensAsync at {Time}", DateTimeOffset.UtcNow);
        token.ThrowIfCancellationRequested();

        logger.LogInformation("Building query for network tokens using filters: Symbol = {Symbol}, Description = {Description}",
            filter.Symbol, filter.Description);
        IQueryable<GetNetworkTokenResponse> query = dbContext.NetworkTokens
            .AsNoTracking()
            .ApplyFilter(filter.Symbol, x => x.Symbol)
            .ApplyFilter(filter.Description, x => x.Description)
            .Select(x => x.ToRead());

        logger.LogInformation("Counting total network tokens matching the filter criteria.");
        int totalCount = await query.CountAsync(token);
        logger.LogInformation("Total count of network tokens: {TotalCount}", totalCount);

        logger.LogInformation("Applying pagination: PageNumber = {PageNumber}, PageSize = {PageSize}",
            filter.PageNumber, filter.PageSize);
        var pagedResult = PagedResponse<IEnumerable<GetNetworkTokenResponse>>.Create(
            filter.PageSize,
            filter.PageNumber,
            totalCount,
            query.Page(filter.PageNumber, filter.PageSize));

        logger.LogInformation("Finishing GetNetworkTokensAsync at {Time}", DateTimeOffset.UtcNow);
        return Result<PagedResponse<IEnumerable<GetNetworkTokenResponse>>>.Success(pagedResult);
    }

    /// <summary>
    /// Retrieves detailed information about a specific network token.
    /// </summary>
    public async Task<Result<GetNetworkTokenDetailResponse>> GetNetworkTokenDetailAsync(Guid networkTokenId, CancellationToken token)
    {
        logger.LogInformation("Starting GetNetworkTokenDetailAsync for NetworkTokenId: {NetworkTokenId} at {Time}", networkTokenId, DateTimeOffset.UtcNow);
        token.ThrowIfCancellationRequested();

        GetNetworkTokenDetailResponse? networkToken = await dbContext.NetworkTokens
            .AsNoTracking()
            .Where(x => x.Id == networkTokenId)
            .Select(x => x.ToReadDetail())
            .FirstOrDefaultAsync(token);

        if (networkToken is not null)
        {
            logger.LogInformation("Successfully retrieved details for NetworkTokenId: {NetworkTokenId}", networkTokenId);
            return Result<GetNetworkTokenDetailResponse>.Success(networkToken);
        }
        else
        {
            logger.LogWarning("Network token not found for NetworkTokenId: {NetworkTokenId}", networkTokenId);
            return Result<GetNetworkTokenDetailResponse>.Failure(ResultPatternError.NotFound("Network Token not found"));
        }
    }

    /// <summary>
    /// Creates a new network token.
    /// </summary>
    public async Task<Result<CreateNetworkTokenResponse>> CreateNetworkTokenAsync(CreateNetworkTokenRequest request, CancellationToken token)
    {
        logger.LogInformation("Starting CreateNetworkTokenAsync at {Time}", DateTimeOffset.UtcNow);
        token.ThrowIfCancellationRequested();

        logger.LogInformation("Checking if network token with Symbol = {Symbol} and NetworkId = {NetworkId} already exists.", request.Symbol, request.NetworkId);
        bool tokenExists = await dbContext.NetworkTokens
            .AnyAsync(x => x.Symbol == request.Symbol && x.NetworkId == request.NetworkId, token);
        if (tokenExists)
        {
            logger.LogWarning("Network token already exists with Symbol: {Symbol} on NetworkId: {NetworkId}.", request.Symbol, request.NetworkId);
            return Result<CreateNetworkTokenResponse>.Failure(ResultPatternError.Conflict("Network Token already exists"));
        }

        logger.LogInformation("Mapping CreateNetworkTokenRequest to NetworkToken entity.");
        NetworkToken newToken = request.ToEntity(accessor);
        logger.LogInformation("Adding new network token entity to the database.");
        await dbContext.NetworkTokens.AddAsync(newToken, token);
        int res = await dbContext.SaveChangesAsync(token);

        if (res != 0)
        {
            logger.LogInformation("Network token created successfully with ID: {NetworkTokenId}", newToken.Id);
            return Result<CreateNetworkTokenResponse>.Success(new CreateNetworkTokenResponse(newToken.Id));
        }
        else
        {
            logger.LogError("Failed to save new network token to the database.");
            return Result<CreateNetworkTokenResponse>.Failure(ResultPatternError.InternalServerError("Data not saved"));
        }
    }

    /// <summary>
    /// Updates an existing network token.
    /// </summary>
    public async Task<Result<UpdateNetworkTokenResponse>> UpdateNetworkTokenAsync(Guid networkTokenId, UpdateNetworkTokenRequest request, CancellationToken token = default)
    {
        logger.LogInformation("Starting UpdateNetworkTokenAsync for NetworkTokenId: {NetworkTokenId} at {Time}", networkTokenId, DateTimeOffset.UtcNow);
        token.ThrowIfCancellationRequested();

        logger.LogInformation("Retrieving network token with ID: {NetworkTokenId} from database.", networkTokenId);
        NetworkToken? networkToken = await dbContext.NetworkTokens.FirstOrDefaultAsync(x => x.Id == networkTokenId, token);
        if (networkToken is null)
        {
            logger.LogWarning("Network token not found for NetworkTokenId: {NetworkTokenId}", networkTokenId);
            return Result<UpdateNetworkTokenResponse>.Failure(ResultPatternError.NotFound("Network Token not found"));
        }

        // Check if new symbol is provided and differs from current one
        if (!string.IsNullOrEmpty(request.Symbol) && request.Symbol != networkToken.Symbol)
        {
            logger.LogInformation("Checking if new symbol {NewSymbol} is already in use for the same network.", request.Symbol);
            bool symbolExists = await dbContext.NetworkTokens
                .AnyAsync(x => x.Symbol == request.Symbol && x.NetworkId == networkToken.NetworkId && x.Id != networkTokenId, token);
            if (symbolExists)
            {
                logger.LogWarning("Network token symbol {NewSymbol} already exists.", request.Symbol);
                return Result<UpdateNetworkTokenResponse>.Failure(ResultPatternError.Conflict("Network Token symbol already exists"));
            }
        }

        logger.LogInformation("Mapping update request to network token entity.");
        networkToken.ToEntity(accessor, request);
        int res = await dbContext.SaveChangesAsync(token);

        if (res != 0)
        {
            logger.LogInformation("Network token updated successfully for NetworkTokenId: {NetworkTokenId}", networkTokenId);
            return Result<UpdateNetworkTokenResponse>.Success(new UpdateNetworkTokenResponse(networkTokenId));
        }
        else
        {
            logger.LogError("Failed to update network token for NetworkTokenId: {NetworkTokenId}", networkTokenId);
            return Result<UpdateNetworkTokenResponse>.Failure(ResultPatternError.InternalServerError("Couldn't update Network Token"));
        }
    }

    /// <summary>
    /// Deletes a network token.
    /// </summary>
    public async Task<Result<DeleteNetworkTokenResponse>> DeleteNetworkTokenAsync(Guid networkTokenId, CancellationToken token = default)
    {
        logger.LogInformation("Starting DeleteNetworkTokenAsync for NetworkTokenId: {NetworkTokenId} at {Time}", networkTokenId, DateTimeOffset.UtcNow);
        token.ThrowIfCancellationRequested();

        logger.LogInformation("Retrieving network token with ID: {NetworkTokenId}", networkTokenId);
        NetworkToken? networkToken = await dbContext.NetworkTokens.FirstOrDefaultAsync(x => x.Id == networkTokenId, token);
        if (networkToken is null)
        {
            logger.LogWarning("Network token not found for NetworkTokenId: {NetworkTokenId}", networkTokenId);
            return Result<DeleteNetworkTokenResponse>.Failure(ResultPatternError.NotFound("Network Token not found"));
        }

        logger.LogInformation("Mapping network token entity for deletion.");
        networkToken.ToEntity(accessor);
        int res = await dbContext.SaveChangesAsync(token);

        if (res != 0)
        {
            logger.LogInformation("Network token with ID: {NetworkTokenId} deleted successfully.", networkTokenId);
            return Result<DeleteNetworkTokenResponse>.Success(new DeleteNetworkTokenResponse(networkTokenId));
        }
        else
        {
            logger.LogError("Failed to delete network token with ID: {NetworkTokenId}", networkTokenId);
            return Result<DeleteNetworkTokenResponse>.Failure(ResultPatternError.InternalServerError("Couldn't delete Network Token"));
        }
    }
}