namespace Infrastructure.ImplementationContract;

public sealed class WalletLinkedAccountService(
    DataContext dbContext,
    IHttpContextAccessor accessor,
    ILogger<WalletLinkedAccountService> logger) : IWalletLinkedAccountService
{
    public async Task<BaseResult> CreateAsync(CreateWalletLinkedAccountRequest request,
        CancellationToken token = default)
    {
        DateTimeOffset date = DateTimeOffset.UtcNow;
        logger.OperationStarted(nameof(CreateAsync), date);

        try
        {
            Guid userId = accessor.GetId();

            User? user = await dbContext.Users
                .AsTracking()
                .FirstOrDefaultAsync(x => x.Id == userId, token);
            if (user is null)
            {
                logger.OperationCompleted(nameof(CreateAsync), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
                return BaseResult.Failure(ResultPatternError.NotFound(Messages.UserNotFound));
            }

            Network? network = await dbContext.Networks
                .AsTracking()
                .FirstOrDefaultAsync(x => x.Name == request.Network, token);
            if (network is null)
            {
                logger.OperationCompleted(nameof(CreateAsync), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
                return BaseResult.Failure(ResultPatternError.NotFound(Messages.NetworkNotFound));
            }

            bool alreadyLinked = await dbContext.WalletLinkedAccounts.AnyAsync(x =>
                x.UserId == user.Id &&
                x.NetworkId == network.Id &&
                x.PublicKey == request.WalletAddress, token);

            if (alreadyLinked)
            {
                logger.OperationCompleted(nameof(CreateAsync), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
                return BaseResult.Failure(ResultPatternError.AlreadyExist(Messages.WalletLinkedAccountAlreadyExist));
            }

            await dbContext.WalletLinkedAccounts.AddAsync(request.ToEntity(network.Id, accessor), token);
            int saved = await dbContext.SaveChangesAsync(token);

            if (saved > 0)
            {
                logger.OperationCompleted(nameof(CreateAsync), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
                return BaseResult.Success();
            }

            logger.OperationCompleted(nameof(CreateAsync), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
            return BaseResult.Failure(ResultPatternError.InternalServerError(Messages.WalletLinkedAccountFailed));
        }
        catch (Exception ex)
        {
            logger.OperationException(nameof(CreateAsync), ex.Message);
            logger.OperationCompleted(nameof(CreateAsync), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
            return BaseResult.Failure(
                ResultPatternError.InternalServerError(Messages.WalletLinkedAccountFailed));
        }
    }

    public async Task<Result<IEnumerable<GetWalletLinkedAccountDetailResponse>>> GetAsync(
        CancellationToken token = default)
        => Result<IEnumerable<GetWalletLinkedAccountDetailResponse>>.Success(
            await dbContext.WalletLinkedAccounts
                .AsNoTracking().Include(x => x.Network)
                .Where(x => x.UserId == accessor.GetId())
                .Select(x => x.ToRead()).ToListAsync(token));
}