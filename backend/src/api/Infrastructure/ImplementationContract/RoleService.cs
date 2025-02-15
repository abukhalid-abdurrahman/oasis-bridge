namespace Infrastructure.ImplementationContract;

public sealed class RoleService(
    DataContext dbContext,
    IHttpContextAccessor accessor) : IRoleService
{
    public async Task<Result<PagedResponse<IEnumerable<GetRolesResponse>>>> GetRolesAsync(RoleFilter filter,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        IQueryable<GetRolesResponse> rolesQuery = dbContext.Roles.AsNoTracking()
            .ApplyFilter(filter.Name, x => x.Name)
            .ApplyFilter(filter.Keyword, x => x.RoleKey)
            .ApplyFilter(filter.Description, x => x.Description)
            .Select(x => x.ToRead());

        int totalCount = await rolesQuery.CountAsync(token);


        PagedResponse<IEnumerable<GetRolesResponse>> result =
            PagedResponse<IEnumerable<GetRolesResponse>>.Create(
                filter.PageSize,
                filter.PageNumber,
                totalCount,
                rolesQuery.Page(filter.PageNumber, filter.PageSize));

        return Result<PagedResponse<IEnumerable<GetRolesResponse>>>.Success(result);
    }


    public async Task<Result<GetRoleDetailResponse>> GetRoleDetailAsync(Guid roleId, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        GetRoleDetailResponse? role = await dbContext.Roles.AsNoTracking()
            .Where(x => x.Id == roleId)
            .Select(x => x.ToReadDetail())
            .FirstOrDefaultAsync(token);

        return role is not null
            ? Result<GetRoleDetailResponse>.Success(role)
            : Result<GetRoleDetailResponse>.Failure(ResultPatternError.NotFound("Role not found"));
    }


    public async Task<Result<CreateRoleResponse>> CreateRoleAsync(CreateRoleRequest request,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        bool roleExists = await dbContext.Roles
            .AnyAsync(x => x.Name == request.RoleName || x.RoleKey == request.RoleKey, token);

        if (roleExists)
            return Result<CreateRoleResponse>.Failure(
                ResultPatternError.Conflict("RoleName or RoleKey already exists"));

        Role newRole = request.ToEntity(accessor);
        await dbContext.Roles.AddAsync(newRole, token);
        int res = await dbContext.SaveChangesAsync(token);

        return res != 0
            ? Result<CreateRoleResponse>.Success(new CreateRoleResponse(newRole.Id))
            : Result<CreateRoleResponse>.Failure(ResultPatternError.InternalServerError("Data not saved"));
    }

    public async Task<Result<UpdateRoleResponse>> UpdateRoleAsync(Guid roleId, UpdateRoleRequest request,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        Role? role = await dbContext.Roles.FirstOrDefaultAsync(x => x.Id == roleId, token);
        if (role is null)
            return Result<UpdateRoleResponse>.Failure(ResultPatternError.NotFound("Role not found"));

        if (!string.IsNullOrEmpty(request.RoleName) && request.RoleName != role.Name)
        {
            bool roleNameExists = await dbContext.Roles
                .AnyAsync(x => x.Name == request.RoleName && x.Id != roleId, token);
            if (roleNameExists)
                return Result<UpdateRoleResponse>.Failure(ResultPatternError.Conflict("Role name already exists"));
        }

        if (!string.IsNullOrEmpty(request.RoleKey) && request.RoleKey != role.RoleKey)
        {
            bool roleKeyExists = await dbContext.Roles
                .AnyAsync(x => x.RoleKey == request.RoleKey && x.Id != roleId, token);
            if (roleKeyExists)
                return Result<UpdateRoleResponse>.Failure(ResultPatternError.Conflict("Role key already exists"));
        }

        if (request.Description is not null)
            role.Description = request.Description;

        dbContext.Roles.Update(role.ToEntity(accessor, request));
        int res = await dbContext.SaveChangesAsync(token);

        return res != 0
            ? Result<UpdateRoleResponse>.Success(new(roleId))
            : Result<UpdateRoleResponse>.Failure(ResultPatternError.InternalServerError("Couldn't update role'"));
    }


    public async Task<BaseResult> DeleteRoleAsync(Guid roleId, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        Role? role = await dbContext.Roles.FirstOrDefaultAsync(x => x.Id == roleId, token);

        if (role is null) return BaseResult.Failure(ResultPatternError.NotFound("Role not found"));

        role.ToEntity(accessor);
        int res = await dbContext.SaveChangesAsync(token);

        return res != 0
            ? Result<UpdateRoleResponse>.Success(new(roleId))
            : Result<UpdateRoleResponse>.Failure(ResultPatternError.InternalServerError("Couldn't delete role'"));
    }
}