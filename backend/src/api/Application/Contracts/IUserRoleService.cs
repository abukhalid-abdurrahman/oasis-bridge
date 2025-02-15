using System.Collections;

namespace Application.Contracts;

public interface IUserRoleService
{
    Task<Result<PagedResponse<IEnumerable>>>
        GetUserRolesAsync(UserRoleFilter filter, CancellationToken token = default);

    Task<Result<GetUserRoleDetailResponse>>
        GetUserRoleDetailAsync(GetUserRoleDetailRequest request, CancellationToken token = default);

    Task<Result<CreateUserRoleResponse>>
        CreateUserRoleAsync(CreateUserRoleRequest request, CancellationToken token = default);

    Task<Result<UpdateUserRoleResponse>> UpdateUserRoleAsync(UpdateUserRoleRequest request,
        CancellationToken token = default);

    Task<BaseResult> DeleteUserRoleAsync(DeleteUserRoleRequest request, CancellationToken token = default);
}