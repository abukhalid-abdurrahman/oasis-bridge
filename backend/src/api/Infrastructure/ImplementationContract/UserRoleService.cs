namespace Infrastructure.ImplementationContract;

public sealed class UserRoleService(
    DataContext dbContext,
    IHttpContextAccessor accessor,
    ILogger<UserRoleService> logger) : IUserRoleService
{
    public async Task<Result<PagedResponse<IEnumerable<GetUserRolesResponse>>>> GetUserRolesAsync(UserRoleFilter filter,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        logger.LogInformation("Fetching user roles with filter: {@Filter}", filter);

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
        logger.LogInformation("Total user roles found: {TotalCount}", totalCount);

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
        logger.LogInformation("Fetching user role details for UserId: {UserId}", id);

        GetUserRoleDetailResponse? userRole = await dbContext.UserRoles
            .Include(x => x.User)
            .Include(x => x.Role).AsNoTracking()
            .Where(x => x.UserId == id)
            .Select(x => x.ToReadDetail())
            .FirstOrDefaultAsync(token);

        if (userRole is not null)
        {
            logger.LogInformation("User role details found for UserId: {UserId}", id);
            return Result<GetUserRoleDetailResponse>.Success(userRole);
        }

        logger.LogWarning("User role not found for UserId: {UserId}", id);
        return Result<GetUserRoleDetailResponse>.Failure(ResultPatternError.NotFound("Role not found"));
    }

    public async Task<Result<CreateUserRoleResponse>> CreateUserRoleAsync(CreateUserRoleRequest request,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        logger.LogInformation("Creating user role for UserId: {UserId}, RoleId: {RoleId}", request.UserId,
            request.RoleId);

        if (!await dbContext.Roles.AnyAsync(x => x.Id == request.RoleId, token))
        {
            logger.LogWarning("Role not found: {RoleId}", request.RoleId);
            return Result<CreateUserRoleResponse>.Failure(ResultPatternError.NotFound("Role not found"));
        }

        if (!await dbContext.Users.AnyAsync(x => x.Id == request.UserId, token))
        {
            logger.LogWarning("User not found: {UserId}", request.UserId);
            return Result<CreateUserRoleResponse>.Failure(ResultPatternError.NotFound("User not found"));
        }

        if (await dbContext.UserRoles.AnyAsync(x => x.UserId == request.UserId && x.RoleId == request.RoleId, token))
        {
            logger.LogWarning("UserRole already exists for UserId: {UserId}, RoleId: {RoleId}", request.UserId,
                request.RoleId);
            return Result<CreateUserRoleResponse>.Failure(ResultPatternError.Conflict("Already exist"));
        }

        UserRole newUserRole = request.ToEntity(accessor);
        await dbContext.UserRoles.AddAsync(newUserRole, token);
        int res = await dbContext.SaveChangesAsync(token);

        if (res != 0)
        {
            logger.LogInformation("UserRole created successfully with Id: {UserRoleId}", newUserRole.Id);
            return Result<CreateUserRoleResponse>.Success(new CreateUserRoleResponse(newUserRole.Id));
        }

        logger.LogError("Failed to save UserRole for UserId: {UserId}, RoleId: {RoleId}", request.UserId,
            request.RoleId);
        return Result<CreateUserRoleResponse>.Failure(ResultPatternError.InternalServerError("Data not saved"));
    }

    public async Task<Result<UpdateUserRoleResponse>> UpdateUserRoleAsync(Guid id, UpdateUserRoleRequest request,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        logger.LogInformation("Updating UserRole with Id: {UserRoleId}", id);

        bool roleExists = await dbContext.Roles.AnyAsync(x => x.Id == request.RoleId, token);
        if (!roleExists)
        {
            logger.LogWarning("Role not found: {RoleId}", request.RoleId);
            return Result<UpdateUserRoleResponse>.Failure(ResultPatternError.NotFound("Role not found"));
        }

        bool userExists = await dbContext.Users.AnyAsync(x => x.Id == request.UserId, token);
        if (!userExists)
        {
            logger.LogWarning("User not found: {UserId}", request.UserId);
            return Result<UpdateUserRoleResponse>.Failure(ResultPatternError.NotFound("User not found"));
        }

        UserRole? userRole = await dbContext.UserRoles.FirstOrDefaultAsync(x => x.Id == id, token);
        if (userRole is null)
        {
            logger.LogWarning("UserRole not found for Id: {UserRoleId}", id);
            return Result<UpdateUserRoleResponse>.Failure(ResultPatternError.NotFound("UserRole not found"));
        }

        if (userRole.UserId == request.UserId && userRole.RoleId == request.RoleId)
        {
            logger.LogInformation("UserRole is already up to date for Id: {UserRoleId}", id);
            return Result<UpdateUserRoleResponse>.Success(new(id));
        }

        bool roleAlreadyAssigned =
            await dbContext.UserRoles.AnyAsync(x => x.UserId == request.UserId && x.RoleId == request.RoleId, token);
        if (roleAlreadyAssigned)
        {
            logger.LogWarning("User already has this role: UserId {UserId}, RoleId {RoleId}", request.UserId,
                request.RoleId);
            return Result<UpdateUserRoleResponse>.Failure(ResultPatternError.Conflict("User already has this role."));
        }

        userRole.ToEntity(accessor, request);
        int res = await dbContext.SaveChangesAsync(token);

        if (res != 0)
        {
            logger.LogInformation("UserRole updated successfully for Id: {UserRoleId}", id);
            return Result<UpdateUserRoleResponse>.Success(new UpdateUserRoleResponse(userRole.Id));
        }

        logger.LogError("Failed to update UserRole with Id: {UserRoleId}", id);
        return Result<UpdateUserRoleResponse>.Failure(ResultPatternError.InternalServerError("Data not saved"));
    }

    public async Task<BaseResult> DeleteUserRoleAsync(Guid id, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        logger.LogInformation("Attempting to delete UserRole with Id: {UserRoleId}", id);

        UserRole? userRole = await dbContext.UserRoles.FirstOrDefaultAsync(x => x.Id == id, token);
        if (userRole is null)
        {
            logger.LogWarning("UserRole not found for Id: {UserRoleId}", id);
            return BaseResult.Failure(ResultPatternError.NotFound("UserRole not found"));
        }

        userRole.ToEntity(accessor);
        int res = await dbContext.SaveChangesAsync(token);

        if (res != 0)
        {
            logger.LogInformation("UserRole deleted successfully with Id: {UserRoleId}", id);
            return BaseResult.Success();
        }

        logger.LogError("Failed to delete UserRole with Id: {UserRoleId}", id);
        return BaseResult.Failure(ResultPatternError.InternalServerError("Couldn't delete UserRole"));
    }
}