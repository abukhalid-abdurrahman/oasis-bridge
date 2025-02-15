namespace Infrastructure.ImplementationContract;

public sealed class UserRoleService(
    DataContext dbContext,
    ILogger<UserRoleService> logger,
    IHttpContextAccessor accessor) : IUserRoleService
{
    public async Task<Result<PagedResponse<IEnumerable<GetUserRolesResponse>>>> GetUserRolesAsync(UserRoleFilter filter,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        IQueryable<GetUserRolesResponse> userRolesQuery = dbContext.UserRoles
            .Include(x => x.User)
            .Include(x => x.Role).AsNoTracking().AsQueryable()
            .ApplyFilter(filter.RoleName, x => x.Role.Name)
            .ApplyFilter(filter.RoleKeyword, x => x.Role.RoleKey)
            .ApplyFilter(filter.RoleDescription, x => x.Role.Description)
            .ApplyFilter(filter.FirstName, x => x.User.FirstName)
            .ApplyFilter(filter.LastName, x => x.User.LastName)
            .ApplyFilter(filter.UserName, x => x.User.UserName)
            .ApplyFilter(filter.PhoneNumber, x => x.User.PhoneNumber)
            .ApplyFilter(filter.Email, x => x.User.Email)
            .Select(x => x.ToRead());

        int totalCount = await userRolesQuery.CountAsync(token);

        PagedResponse<IEnumerable<GetUserRolesResponse>> result =
            PagedResponse<IEnumerable<GetUserRolesResponse>>.Create(
                filter.PageSize,
                filter.PageNumber,
                totalCount,
                userRolesQuery.Page(filter.PageNumber, filter.PageSize));

        return Result<PagedResponse<IEnumerable<GetUserRolesResponse>>>.Success(result);
    }

    public async Task<Result<GetUserRoleDetailResponse>> GetUserRoleDetailAsync(Guid id,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        GetUserRoleDetailResponse? userRole = await dbContext.UserRoles.AsNoTracking()
            .Where(x => x.UserId == id)
            .Select(x => x.ToReadDetail())
            .FirstOrDefaultAsync(token);

        return userRole is not null
            ? Result<GetUserRoleDetailResponse>.Success(userRole)
            : Result<GetUserRoleDetailResponse>.Failure(ResultPatternError.NotFound("Role not found"));
    }

    public async Task<Result<CreateUserRoleResponse>> CreateUserRoleAsync(CreateUserRoleRequest request,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        bool roleExists = await dbContext.Roles
            .AnyAsync(x => x.Id == request.RoleId, token);
        if (!roleExists)
            return Result<CreateUserRoleResponse>.Failure(
                ResultPatternError.NotFound("Role not found"));


        bool userExists = await dbContext.Users
            .AnyAsync(x => x.Id == request.UserId, token);
        if (!userExists)
            return Result<CreateUserRoleResponse>.Failure(
                ResultPatternError.NotFound("User not found"));

        bool conflict = await dbContext.UserRoles.AnyAsync(x
            => x.UserId == request.UserId && x.RoleId == request.RoleId, token);
        if (conflict)
            return Result<CreateUserRoleResponse>.Failure(
                ResultPatternError.Conflict("Already exist"));

        UserRole newUserRole = request.ToEntity(accessor);
        await dbContext.UserRoles.AddAsync(newUserRole, token);
        int res = await dbContext.SaveChangesAsync(token);

        return res != 0
            ? Result<CreateUserRoleResponse>.Success(new CreateUserRoleResponse(newUserRole.Id))
            : Result<CreateUserRoleResponse>.Failure(ResultPatternError.InternalServerError("Data not saved"));
    }

    public async Task<Result<UpdateUserRoleResponse>> UpdateUserRoleAsync(Guid id, UpdateUserRoleRequest request,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        bool roleExists = await dbContext.Roles
            .AnyAsync(x => x.Id == request.RoleId, token);
        if (!roleExists)
            return Result<UpdateUserRoleResponse>.Failure(
                ResultPatternError.NotFound("Role not found"));


        bool userExists = await dbContext.Users
            .AnyAsync(x => x.Id == request.UserId, token);
        if (!userExists)
            return Result<UpdateUserRoleResponse>.Failure(
                ResultPatternError.NotFound("User not found"));

        UserRole? userRole = await dbContext.UserRoles.FirstOrDefaultAsync(x
            => x.Id == id, token);
        if (userRole is null)
            return Result<UpdateUserRoleResponse>.Failure(
                ResultPatternError.NotFound("UserRole not found"));

        if (userRole.UserId == request.UserId && userRole.RoleId == request.RoleId)
            return Result<UpdateUserRoleResponse>.Success(new(id));

        bool roleAlreadyAssigned = await dbContext.UserRoles
            .AnyAsync(x => x.UserId == request.UserId && x.RoleId == request.RoleId, token);
        if (roleAlreadyAssigned)
            return Result<UpdateUserRoleResponse>.Failure(
                ResultPatternError.Conflict("User already has this role."));

        UserRole updateUserRole = userRole.ToEntity(accessor, request);

        int res = await dbContext.SaveChangesAsync(token);

        return res != 0
            ? Result<UpdateUserRoleResponse>.Success(new UpdateUserRoleResponse(updateUserRole.Id))
            : Result<UpdateUserRoleResponse>.Failure(ResultPatternError.InternalServerError("Data not saved"));
    }

    public async Task<BaseResult> DeleteUserRoleAsync(Guid id, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        UserRole? userRole = await dbContext.UserRoles.FirstOrDefaultAsync(x => x.Id == id, token);

        if (userRole is null) return BaseResult.Failure(ResultPatternError.NotFound("UserRole not found"));

        userRole.ToEntity(accessor);
        int res = await dbContext.SaveChangesAsync(token);

        return res != 0
            ? BaseResult.Success()
            : BaseResult.Failure(ResultPatternError.InternalServerError("Couldn't delete UserRole'"));
    }
}