using Role = Domain.Entities.Role;

namespace Infrastructure.ImplementationContract;

public sealed class RoleService(
    DataContext dbContext,
    IHttpContextAccessor accessor,
    ILogger<RoleService> logger) : IRoleService
{
    public async Task<Result<PagedResponse<IEnumerable<GetRolesResponse>>>> GetRolesAsync(RoleFilter filter,
        CancellationToken token = default)
    {
        logger.LogInformation("Starting GetRolesAsync at {Time}", DateTimeOffset.UtcNow);
        token.ThrowIfCancellationRequested();

        logger.LogInformation("Applying filters: Name = {Name}, Keyword = {Keyword}, Description = {Description}",
            filter.Name, filter.Keyword, filter.Description);

        IQueryable<GetRolesResponse> rolesQuery = dbContext.Roles.AsNoTracking()
            .ApplyFilter(filter.Name, x => x.Name)
            .ApplyFilter(filter.Keyword, x => x.RoleKey)
            .ApplyFilter(filter.Description, x => x.Description)
            .Select(x => x.ToRead());

        int totalCount = await rolesQuery.CountAsync(token);
        logger.LogInformation("Total roles found: {TotalCount}", totalCount);

        PagedResponse<IEnumerable<GetRolesResponse>> result = PagedResponse<IEnumerable<GetRolesResponse>>.Create(
            filter.PageSize,
            filter.PageNumber,
            totalCount,
            rolesQuery.Page(filter.PageNumber, filter.PageSize));

        logger.LogInformation("Returning paginated list of roles.");
        return Result<PagedResponse<IEnumerable<GetRolesResponse>>>.Success(result);
    }

    public async Task<Result<GetRoleDetailResponse>> GetRoleDetailAsync(Guid roleId, CancellationToken token = default)
    {
        logger.LogInformation("Starting GetRoleDetailAsync for RoleId: {RoleId}", roleId);
        token.ThrowIfCancellationRequested();

        GetRoleDetailResponse? role = await dbContext.Roles.AsNoTracking()
            .Where(x => x.Id == roleId)
            .Select(x => x.ToReadDetail())
            .FirstOrDefaultAsync(token);

        if (role is not null)
        {
            logger.LogInformation("Successfully retrieved details for RoleId: {RoleId}", roleId);
            return Result<GetRoleDetailResponse>.Success(role);
        }
        else
        {
            logger.LogWarning("Role not found for RoleId: {RoleId}", roleId);
            return Result<GetRoleDetailResponse>.Failure(ResultPatternError.NotFound("Role not found"));
        }
    }

    public async Task<Result<CreateRoleResponse>> CreateRoleAsync(CreateRoleRequest request,
        CancellationToken token = default)
    {
        logger.LogInformation("Starting CreateRoleAsync at {Time}", DateTimeOffset.UtcNow);
        token.ThrowIfCancellationRequested();

        logger.LogInformation("Checking if role with Name = {RoleName} or Key = {RoleKey} already exists.",
            request.RoleName, request.RoleKey);

        bool roleExists = await dbContext.Roles
            .AnyAsync(x => x.Name == request.RoleName || x.RoleKey == request.RoleKey, token);

        if (roleExists)
        {
            logger.LogWarning("Role already exists with Name: {RoleName} or Key: {RoleKey}.", request.RoleName, request.RoleKey);
            return Result<CreateRoleResponse>.Failure(ResultPatternError.Conflict("RoleName or RoleKey already exists"));
        }

        logger.LogInformation("Mapping CreateRoleRequest to Role entity.");
        Role newRole = request.ToEntity(accessor);

        logger.LogInformation("Adding new role entity to the database.");
        await dbContext.Roles.AddAsync(newRole, token);
        int res = await dbContext.SaveChangesAsync(token);

        if (res != 0)
        {
            logger.LogInformation("Role created successfully with ID: {RoleId}", newRole.Id);
            return Result<CreateRoleResponse>.Success(new CreateRoleResponse(newRole.Id));
        }
        else
        {
            logger.LogError("Failed to save new role to the database.");
            return Result<CreateRoleResponse>.Failure(ResultPatternError.InternalServerError("Data not saved"));
        }
    }

    public async Task<Result<UpdateRoleResponse>> UpdateRoleAsync(Guid roleId, UpdateRoleRequest request,
        CancellationToken token = default)
    {
        logger.LogInformation("Starting UpdateRoleAsync for RoleId: {RoleId}", roleId);
        token.ThrowIfCancellationRequested();

        logger.LogInformation("Retrieving role with ID: {RoleId} from database.", roleId);
        Role? role = await dbContext.Roles.FirstOrDefaultAsync(x => x.Id == roleId, token);

        if (role is null)
        {
            logger.LogWarning("Role not found for RoleId: {RoleId}", roleId);
            return Result<UpdateRoleResponse>.Failure(ResultPatternError.NotFound("Role not found"));
        }

        if (!string.IsNullOrEmpty(request.RoleName) && request.RoleName != role.Name)
        {
            logger.LogInformation("Checking if new role name {NewRoleName} is already in use.", request.RoleName);
            bool roleNameExists = await dbContext.Roles
                .AnyAsync(x => x.Name == request.RoleName && x.Id != roleId, token);
            if (roleNameExists)
            {
                logger.LogWarning("Role name {NewRoleName} already exists.", request.RoleName);
                return Result<UpdateRoleResponse>.Failure(ResultPatternError.Conflict("Role name already exists"));
            }
        }

        if (!string.IsNullOrEmpty(request.RoleKey) && request.RoleKey != role.RoleKey)
        {
            logger.LogInformation("Checking if new role key {NewRoleKey} is already in use.", request.RoleKey);
            bool roleKeyExists = await dbContext.Roles
                .AnyAsync(x => x.RoleKey == request.RoleKey && x.Id != roleId, token);
            if (roleKeyExists)
            {
                logger.LogWarning("Role key {NewRoleKey} already exists.", request.RoleKey);
                return Result<UpdateRoleResponse>.Failure(ResultPatternError.Conflict("Role key already exists"));
            }
        }

        logger.LogInformation("Updating role entity with new values.");
        role.ToEntity(accessor, request);
        int res = await dbContext.SaveChangesAsync(token);

        if (res != 0)
        {
            logger.LogInformation("Role updated successfully for RoleId: {RoleId}", roleId);
            return Result<UpdateRoleResponse>.Success(new(roleId));
        }
        else
        {
            logger.LogError("Failed to update role for RoleId: {RoleId}", roleId);
            return Result<UpdateRoleResponse>.Failure(ResultPatternError.InternalServerError("Couldn't update role"));
        }
    }

    public async Task<BaseResult> DeleteRoleAsync(Guid roleId, CancellationToken token = default)
    {
        logger.LogInformation("Starting DeleteRoleAsync for RoleId: {RoleId}", roleId);
        token.ThrowIfCancellationRequested();

        logger.LogInformation("Retrieving role with ID: {RoleId}", roleId);
        Role? role = await dbContext.Roles.FirstOrDefaultAsync(x => x.Id == roleId, token);

        if (role is null)
        {
            logger.LogWarning("Role not found for RoleId: {RoleId}", roleId);
            return BaseResult.Failure(ResultPatternError.NotFound("Role not found"));
        }

        logger.LogInformation("Deleting role entity.");
        role.ToEntity(accessor);
        int res = await dbContext.SaveChangesAsync(token);

        if (res != 0)
        {
            logger.LogInformation("Role with ID: {RoleId} deleted successfully.", roleId);
            return Result<UpdateRoleResponse>.Success(new(roleId));
        }
        else
        {
            logger.LogError("Failed to delete role with ID: {RoleId}", roleId);
            return Result<UpdateRoleResponse>.Failure(ResultPatternError.InternalServerError("Couldn't delete role"));
        }
    }
}
