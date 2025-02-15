namespace Infrastructure.ImplementationContract;

public sealed class RoleService(
    DataContext dbContext,
    ILogger<RoleService> logger,
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
        await dbContext.SaveChangesAsync(token);

        return Result<CreateRoleResponse>.Success(new CreateRoleResponse(newRole.Id));
    }

    public async Task<Result<UpdateRoleResponse>> UpdateRoleAsync(Guid roleId, UpdateRoleRequest request,
        CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResult> DeleteRoleAsync(Guid roleId, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}