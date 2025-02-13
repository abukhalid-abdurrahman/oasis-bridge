namespace Application.Contracts;

public interface IUserService
{
    Task<Result<PagedResponse<GetAllUserResponse>>>
        GetUsersAsync(UserFilter filter, CancellationToken token = default);

    Task<Result<GetUserDetailPublicResponse>>
        GetByIdForUser(Guid userId, CancellationToken token = default);

    Task<Result<GetUserDetailPrivateResponse>>
        GetByIdForSelf(Guid userId, CancellationToken token = default);

    Task<Result<UpdateUserResponse>>
        UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken token = default);
}