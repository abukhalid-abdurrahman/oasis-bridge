namespace Infrastructure.ImplementationContract;

public sealed class UserService(
    DataContext dbContext,
    IHttpContextAccessor accessor,
    IRadixBridge radixBridge,
    ISolanaBridge solanaBridge,
    ILogger<UserService> logger) : IUserService
{
    public async Task<Result<PagedResponse<IEnumerable<GetAllUserResponse>>>> GetUsersAsync(UserFilter filter,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        logger.LogInformation("Fetching users with filter: {@Filter}", filter);

        IQueryable<GetAllUserResponse> users = dbContext.Users.AsNoTracking()
            .ApplyFilter(filter.Email, x => x.Email)
            .ApplyFilter(filter.PhoneNumber, x => x.PhoneNumber)
            .ApplyFilter(filter.FirstName, x => x.FirstName)
            .ApplyFilter(filter.LastName, x => x.LastName)
            .ApplyFilter(filter.UserName, x => x.UserName)
            .Select(x => x.ToRead());

        int totalCount = await users.CountAsync(token);
        logger.LogInformation("Total users found: {TotalCount}", totalCount);

        PagedResponse<IEnumerable<GetAllUserResponse>> result =
            PagedResponse<IEnumerable<GetAllUserResponse>>.Create(
                filter.PageSize,
                filter.PageNumber,
                totalCount,
                users.Page(filter.PageNumber, filter.PageSize));

        return Result<PagedResponse<IEnumerable<GetAllUserResponse>>>.Success(result);
    }

    public async Task<Result<GetUserDetailPublicResponse>> GetByIdForUser(Guid userId,
        CancellationToken token = default)
    {
        logger.LogInformation("Fetching user details for UserId: {UserId}", userId);

        GetUserDetailPublicResponse? response = await dbContext.Users.AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => x.ToReadPublicDetail())
            .FirstOrDefaultAsync(token);

        if (response is not null)
        {
            logger.LogInformation("User details found for UserId: {UserId}", userId);
            return Result<GetUserDetailPublicResponse>.Success(response);
        }

        logger.LogWarning("User not found for UserId: {UserId}", userId);
        return Result<GetUserDetailPublicResponse>.Failure(ResultPatternError.NotFound());
    }

    public async Task<Result<IEnumerable<GetVirtualAccountDetailResponse>>> GetVirtualAccountsAsync(
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        logger.LogInformation("Fetching virtual accounts for current user");

        Guid? userId = accessor.GetId();
        if (userId is null)
        {
            logger.LogWarning("UserId is missing in the request");
            return Result<IEnumerable<GetVirtualAccountDetailResponse>>.Failure(
                ResultPatternError.BadRequest("UserId is required."));
        }

        bool exists = await dbContext.Users.AnyAsync(x => x.Id == userId, token);
        if (!exists)
        {
            logger.LogWarning("User not found: {UserId}", userId);
            return Result<IEnumerable<GetVirtualAccountDetailResponse>>.Failure(
                ResultPatternError.NotFound("User not found."));
        }

        var accounts = await (from nt in dbContext.NetworkTokens
            join n in dbContext.Networks on nt.NetworkId equals n.Id
            join va in dbContext.VirtualAccounts on n.Id equals va.NetworkId
            join u in dbContext.Users on va.UserId equals u.Id
            where u.Id == userId
            select new
            {
                va.Address,
                Network = n.Name,
                Token = nt.Symbol,
            }).ToListAsync(token);

        List<GetVirtualAccountDetailResponse> result = new();

        foreach (var account in accounts)
        {
            decimal accountBalance = account.Token == "SOL"
                ? (await solanaBridge.GetAccountBalanceAsync(account.Address, token)).Value
                : (await radixBridge.GetAccountBalanceAsync(account.Address, token)).Value;

            result.Add(new GetVirtualAccountDetailResponse(
                account.Address,
                account.Network,
                account.Token,
                accountBalance
            ));
        }

        logger.LogInformation("Fetched {AccountCount} virtual accounts for UserId: {UserId}", result.Count, userId);
        return Result<IEnumerable<GetVirtualAccountDetailResponse>>.Success(result);
    }

    public async Task<Result<GetUserDetailPrivateResponse>> GetByIdForSelf(CancellationToken token = default)
    {
        logger.LogInformation("Fetching private details for the current user");

        string? userIdString = accessor.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == CustomClaimTypes.Id)?.Value;

        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            logger.LogWarning("Invalid or missing user ID in claims");
            return Result<GetUserDetailPrivateResponse>.Failure(ResultPatternError.NotFound());
        }

        GetUserDetailPrivateResponse? response = await dbContext.Users.AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => x.ToReadPrivateDetail())
            .FirstOrDefaultAsync(token);

        return response is not null
            ? Result<GetUserDetailPrivateResponse>.Success(response)
            : Result<GetUserDetailPrivateResponse>.Failure(ResultPatternError.NotFound());
    }

    public async Task<Result<UpdateUserResponse>> UpdateProfileAsync(UpdateUserProfileRequest request,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        logger.LogInformation("Starting profile update");

        string? userIdString = accessor.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == CustomClaimTypes.Id)?.Value;

        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            logger.LogWarning("Failed to parse UserId from claims");
            return Result<UpdateUserResponse>.Failure(ResultPatternError.NotFound("Couldn't take user ID"));
        }

        User? user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, token);
        if (user is null)
        {
            logger.LogWarning("User not found: {UserId}", userId);
            return Result<UpdateUserResponse>.Failure(ResultPatternError.NotFound("User not found"));
        }

        if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
        {
            bool emailExists = await dbContext.Users
                .AnyAsync(x => x.Email == request.Email && x.Id != userId, token);
            if (emailExists)
            {
                logger.LogWarning("Email already exists: {Email}", request.Email);
                return Result<UpdateUserResponse>.Failure(ResultPatternError.Conflict("Email already exists"));
            }
        }

        if (!string.IsNullOrEmpty(request.PhoneNumber) && request.PhoneNumber != user.PhoneNumber)
        {
            bool phoneExists = await dbContext.Users
                .AnyAsync(x => x.PhoneNumber == request.PhoneNumber && x.Id != userId, token);
            if (phoneExists)
            {
                logger.LogWarning("PhoneNumber already exists: {PhoneNumber}", request.PhoneNumber);
                return Result<UpdateUserResponse>.Failure(ResultPatternError.Conflict("PhoneNumber already exists"));
            }
        }

        if (!string.IsNullOrEmpty(request.UserName) && request.UserName != user.UserName)
        {
            bool userNameExists = await dbContext.Users
                .AnyAsync(x => x.UserName == request.UserName && x.Id != userId, token);
            if (userNameExists)
            {
                logger.LogWarning("UserName already exists: {UserName}", request.UserName);
                return Result<UpdateUserResponse>.Failure(ResultPatternError.Conflict("UserName already exists"));
            }
        }

        dbContext.Users.Update(user.ToEntity(request, accessor));
        await dbContext.SaveChangesAsync(token);

        logger.LogInformation("User profile updated successfully for UserId: {UserId}", userId);
        return Result<UpdateUserResponse>.Success(new(userId));
    }
}