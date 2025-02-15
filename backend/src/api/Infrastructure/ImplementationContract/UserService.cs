namespace Infrastructure.ImplementationContract;

public sealed class UserService(
    DataContext dbContext,
    ILogger<UserService> logger,
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

            user.Email = request.Email;
        }

        if (!string.IsNullOrEmpty(request.PhoneNumber) && request.PhoneNumber != user.PhoneNumber)
        {
            bool phoneExists = await dbContext.Users
                .AnyAsync(x => x.PhoneNumber == request.PhoneNumber && x.Id != userId, token);
            if (phoneExists)
                return Result<UpdateUserResponse>.Failure(
                    ResultPatternError.Conflict("PhoneNumber already exist"));

            user.PhoneNumber = request.PhoneNumber;
        }

        if (!string.IsNullOrEmpty(request.UserName) && request.UserName != user.UserName)
        {
            bool userNameExists = await dbContext.Users
                .AnyAsync(x => x.UserName == request.UserName && x.Id != userId, token);
            if (userNameExists)
                return Result<UpdateUserResponse>.Failure(
                    ResultPatternError.Conflict("UserName already exists"));

            user.UserName = request.UserName;
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Dob = request.Dob;

        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(token);


        return Result<UpdateUserResponse>.Success(new(userId));
    }
}