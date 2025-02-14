using Application.DTOs.User.Requests;
using Application.DTOs.User.Responses;
using Application.Extensions.Responses.PagedResponse;
using Application.Filters;

namespace Infrastructure.ImplementationContract;

public sealed class UserService:IUserService
{
    public async Task<Result<PagedResponse<List<GetAllUserResponse>>>> GetUsersAsync(UserFilter filter, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<GetUserDetailPublicResponse>> GetByIdForUser(Guid userId, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<GetUserDetailPrivateResponse>> GetByIdForSelf(Guid userId, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<UpdateUserResponse>> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}