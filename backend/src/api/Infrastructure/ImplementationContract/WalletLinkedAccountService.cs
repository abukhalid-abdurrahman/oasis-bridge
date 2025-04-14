namespace Infrastructure.ImplementationContract;

public sealed class WalletLinkedAccountService(
    DataContext dbContext,
    IHttpContextAccessor accessor,
    ILogger<WalletLinkedAccountService> logger) : IWalletLinkedAccountService
{
    public async Task<BaseResult> CreateAsync(CreateWalletLinkedAccountRequest request,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        logger.OperationStarted(nameof(CreateAsync), DateTimeOffset.UtcNow);

        try
        {
            Guid userId = accessor.GetId();

            User? user = await dbContext.Users
                .AsTracking()
                .FirstOrDefaultAsync(x => x.Id == userId, token);
            if (user is null)
            {
                return BaseResult.Failure(ResultPatternError.NotFound("User not found!"));
            }

            Network? network = await dbContext.Networks
                .AsTracking()
                .FirstOrDefaultAsync(x => x.Name == request.Network, token);
            if (network is null)
            {
                return BaseResult.Failure(ResultPatternError.NotFound("Network not found!"));
            }

            bool alreadyLinked = await dbContext.WalletLinkedAccounts.AnyAsync(x =>
                x.UserId == user.Id &&
                x.NetworkId == network.Id &&
                x.PublicKey == request.WalletAddress, token);

            if (alreadyLinked)
            {
                return BaseResult.Failure(ResultPatternError.AlreadyExist("Linked Account already exists"));
            }

            WalletLinkedAccount newLinkedAccount = request.ToEntity(network.Id, accessor);
            await dbContext.WalletLinkedAccounts.AddAsync(newLinkedAccount, token);

            int saved = await dbContext.SaveChangesAsync(token);

            if (saved > 0)
            {
                logger.OperationCompleted(nameof(CreateAsync), DateTimeOffset.UtcNow);
                return BaseResult.Success();
            }

            return BaseResult.Failure(ResultPatternError.InternalServerError("Linked account not created!"));
        }
        catch (Exception ex)
        {
            return BaseResult.Failure(
                ResultPatternError.InternalServerError(Messages.WalletLinkingFailed));
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