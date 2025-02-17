using Application.DTOs.VirtualAccount.Responses;

namespace Infrastructure.ImplementationContract;

public sealed class UserService(
    DataContext dbContext,
    IHttpContextAccessor accessor) : IUserService
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

        IEnumerable<GetVirtualAccountDetailResponse> result = await (from u in dbContext.Users
                join va in dbContext.VirtualAccounts on u.Id equals va.UserId
                join n in dbContext.Networks on va.NetworkId equals n.Id
                join ab in dbContext.AccountBalances on va.Id equals ab.VirtualAccountId
                join nt in dbContext.NetworkTokens on ab.NetworkTokenId equals nt.Id
                where u.Id == userId
                select new GetVirtualAccountDetailResponse(
                    va.Address,
                    n.Name,
                    nt.Symbol,
                    ab.Balance)
            ).ToListAsync(token);
        
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