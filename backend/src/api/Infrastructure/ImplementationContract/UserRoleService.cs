namespace Infrastructure.ImplementationContract;

public sealed class UserRoleService : IUserRoleService
{
    public async Task<Result<PagedResponse<List<GetUserRolesResponse>>>> GetUserRolesAsync(UserRoleFilter filter,
        CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<GetUserRoleDetailResponse>> GetUserRoleDetailAsync(GetUserRoleDetailRequest request,
        CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<CreateUserRoleResponse>> CreateUserRoleAsync(CreateUserRoleRequest request,
        CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<UpdateUserRoleResponse>> UpdateUserRoleAsync(UpdateUserRoleRequest request,
        CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResult> DeleteUserRoleAsync(DeleteUserRoleRequest request, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}