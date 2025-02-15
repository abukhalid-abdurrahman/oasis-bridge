namespace Application.Contracts;

public interface IUserRoleService
{
    Task<Result<PagedResponse<IEnumerable<GetUserRolesResponse>>>>
        GetUserRolesAsync(UserRoleFilter filter, CancellationToken token = default);

    Task<Result<GetUserRoleDetailResponse>>
        GetUserRoleDetailAsync(Guid id, CancellationToken token = default);

    Task<Result<CreateUserRoleResponse>>
        CreateUserRoleAsync(CreateUserRoleRequest request, CancellationToken token = default);

    Task<Result<UpdateUserRoleResponse>> UpdateUserRoleAsync(Guid id,UpdateUserRoleRequest request,
        CancellationToken token = default);

    Task<BaseResult> DeleteUserRoleAsync(Guid id, CancellationToken token = default);
}