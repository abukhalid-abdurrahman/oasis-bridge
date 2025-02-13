namespace Application.Contracts;

public interface IRoleService
{
    Task<Result<PagedResponse<List<GetRolesResponse>>>>
        GetRolesAsync(RoleFilter filter, CancellationToken token = default);

    Task<Result<GetRoleDetailResponse>>
        GetRoleDetailAsync(Guid roleId, CancellationToken token);

    Task<Result<CreateRoleResponse>>
        CreateRoleAsync(CreateRoleRequest request, CancellationToken token);

    Task<Result<UpdateRoleResponse>>
        UpdateRoleAsync(Guid roleId, UpdateRoleRequest request, CancellationToken token = default);

    Task<BaseResult> DeleteRoleAsync(Guid roleId, CancellationToken token = default);
}