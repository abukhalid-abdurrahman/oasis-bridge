using Application.DTOs.Role.Requests;
using Application.DTOs.Role.Responses;
using Application.Extensions.Responses.PagedResponse;
using Application.Filters;

namespace Infrastructure.ImplementationContract;

public sealed class RoleService:IRoleService
{
    public async Task<Result<PagedResponse<List<GetRolesResponse>>>> GetRolesAsync(RoleFilter filter, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<GetRoleDetailResponse>> GetRoleDetailAsync(Guid roleId, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<CreateRoleResponse>> CreateRoleAsync(CreateRoleRequest request, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<UpdateRoleResponse>> UpdateRoleAsync(Guid roleId, UpdateRoleRequest request, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResult> DeleteRoleAsync(Guid roleId, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}