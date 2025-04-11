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
        logger.LogInformation(" Starting linking wallet: {WalletAddress} for network: {Network}", request.WalletAddress,
            request.Network);

        try
        {
            Guid userId = accessor.GetId();
            logger.LogDebug(" Extracted UserId: {UserId}", userId);

            User? user = await dbContext.Users
                .AsTracking()
                .FirstOrDefaultAsync(x => x.Id == userId, token);
            if (user is null)
            {
                logger.LogWarning(" User not found. UserId: {UserId}", userId);
                return BaseResult.Failure(ResultPatternError.NotFound("User not found!"));
            }

            Network? network = await dbContext.Networks
                .AsTracking()
                .FirstOrDefaultAsync(x => x.Name == request.Network, token);
            if (network is null)
            {
                logger.LogWarning(" Network not found. Name: {NetworkName}", request.Network);
                return BaseResult.Failure(ResultPatternError.NotFound("Network not found!"));
            }

            bool alreadyLinked = await dbContext.WalletLinkedAccounts.AnyAsync(x =>
                x.UserId == user.Id &&
                x.NetworkId == network.Id &&
                x.PublicKey == request.WalletAddress, token);

            if (alreadyLinked)
            {
                logger.LogInformation(" Wallet already linked. UserId: {UserId}, Wallet: {WalletAddress}", user.Id,
                    request.WalletAddress);
                return BaseResult.Failure(ResultPatternError.AlreadyExist("Linked Account already exists"));
            }

            WalletLinkedAccount newLinkedAccount = request.ToEntity(network.Id, accessor);
            await dbContext.WalletLinkedAccounts.AddAsync(newLinkedAccount, token);

            int saved = await dbContext.SaveChangesAsync(token);

            if (saved > 0)
            {
                logger.LogInformation(" Linked account created successfully. UserId: {UserId}, Wallet: {WalletAddress}",
                    user.Id, request.WalletAddress);
                return BaseResult.Success();
            }
            else
            {
                logger.LogError(
                    " Failed to create linked account. No changes saved. UserId: {UserId}, Wallet: {WalletAddress}",
                    user.Id, request.WalletAddress);
                return BaseResult.Failure(ResultPatternError.InternalServerError("Linked account not created!"));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                " Exception occurred while creating linked account. Wallet: {WalletAddress}, Network: {Network}",
                request.WalletAddress, request.Network);
            return BaseResult.Failure(
                ResultPatternError.InternalServerError("Unexpected error occurred while creating the linked account."));
        }
    }

    public async Task<Result<IEnumerable<GetWalletLinkedAccountDetailResponse>>> GetAsync(
        CancellationToken token = default)
        => Result<IEnumerable<GetWalletLinkedAccountDetailResponse>>.Success(
            await dbContext.WalletLinkedAccounts
                .AsNoTracking().Include(x => x.Network)
                .Select(x => x.ToRead()).ToListAsync(token));
}