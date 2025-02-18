using Application.DTOs.VirtualAccount.Responses;

namespace Infrastructure.ImplementationContract;

public sealed class UserService(
    DataContext dbContext,
    IHttpContextAccessor accessor,
    IRadixBridge radixBridge,
    ISolanaBridge solanaBridge) : IUserService
{
    public async Task<Result<PagedResponse<IEnumerable<GetAllUserResponse>>>> GetUsersAsync(UserFilter filter,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        IQueryable<GetAllUserResponse> users = dbContext.Users.AsNoTracking()
            .ApplyFilter(filter.Email, x => x.Email)
            .ApplyFilter(filter.PhoneNumber, x => x.PhoneNumber)
            .ApplyFilter(filter.FirstName, x => x.FirstName)
            .ApplyFilter(filter.LastName, x => x.LastName)
            .ApplyFilter(filter.UserName, x => x.UserName)
            .Select(x => x.ToRead());

        int totalCount = await users.CountAsync(token);

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
        GetUserDetailPublicResponse? response = await dbContext.Users.AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => x.ToReadPublicDetail())
            .FirstOrDefaultAsync(token);

        return response is not null
            ? Result<GetUserDetailPublicResponse>.Success(response)
            : Result<GetUserDetailPublicResponse>.Failure(ResultPatternError.NotFound());
    }

    public async Task<Result<IEnumerable<GetVirtualAccountDetailResponse>>> GetVirtualAccountsAsync(
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        Guid? userId = accessor.GetId();
        if (userId is null)
            return Result<IEnumerable<GetVirtualAccountDetailResponse>>.Failure(
                ResultPatternError.BadRequest("UserId is required."));

        bool exists = await dbContext.Users.AnyAsync(x => x.Id == userId, token);
        if (!exists)
            return Result<IEnumerable<GetVirtualAccountDetailResponse>>.Failure(
                ResultPatternError.NotFound("User not found."));

        var accounts = await (from n in dbContext.Networks
            join nt in dbContext.NetworkTokens on n.Id equals nt.NetworkId
            join ac in dbContext.AccountBalances on nt.Id equals ac.NetworkTokenId
            join va in dbContext.VirtualAccounts on ac.VirtualAccountId equals va.Id
            where va.UserId == userId
            select new
            {
                va.Address,
                Network = n.Name,
                Token = nt.Symbol
            }).ToListAsync(token);

        List<GetVirtualAccountDetailResponse> result = new List<GetVirtualAccountDetailResponse>();
        foreach (var account in accounts)
        {
            decimal balance = account.Token == "SOL"
                ? await solanaBridge.GetAccountBalanceAsync(account.Address, token)
                : await radixBridge.GetAccountBalanceAsync(account.Address, token);

            result.Add(new GetVirtualAccountDetailResponse(
                account.Address,
                account.Network,
                account.Token,
                balance
            ));
        }

        return Result<IEnumerable<GetVirtualAccountDetailResponse>>.Success(result);
    }


    public async Task<Result<GetUserDetailPrivateResponse>> GetByIdForSelf(CancellationToken token = default)
    {
        string? userIdString = accessor.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == CustomClaimTypes.Id)?.Value;

        if (!Guid.TryParse(userIdString, out Guid userId))
        {
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

        string? userIdString = accessor.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == CustomClaimTypes.Id)?.Value;

        if (!Guid.TryParse(userIdString, out Guid userId))
            return Result<UpdateUserResponse>.Failure(ResultPatternError.NotFound("Couldn't  take user ID"));


        User? user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, token);
        if (user is null)
            return Result<UpdateUserResponse>.Failure(ResultPatternError.NotFound("User not found"));

        if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
        {
            bool emailExists = await dbContext.Users
                .AnyAsync(x => x.Email == request.Email && x.Id != userId, token);
            if (emailExists)
                return Result<UpdateUserResponse>.Failure(ResultPatternError.Conflict("Email already exists"));
        }

        if (!string.IsNullOrEmpty(request.PhoneNumber) && request.PhoneNumber != user.PhoneNumber)
        {
            bool phoneExists = await dbContext.Users
                .AnyAsync(x => x.PhoneNumber == request.PhoneNumber && x.Id != userId, token);
            if (phoneExists)
                return Result<UpdateUserResponse>.Failure(
                    ResultPatternError.Conflict("PhoneNumber already exist"));
        }

        if (!string.IsNullOrEmpty(request.UserName) && request.UserName != user.UserName)
        {
            bool userNameExists = await dbContext.Users
                .AnyAsync(x => x.UserName == request.UserName && x.Id != userId, token);
            if (userNameExists)
                return Result<UpdateUserResponse>.Failure(
                    ResultPatternError.Conflict("UserName already exists"));
        }


        dbContext.Users.Update(user.ToEntity(request, accessor));
        await dbContext.SaveChangesAsync(token);


        return Result<UpdateUserResponse>.Success(new(userId));
    }
}