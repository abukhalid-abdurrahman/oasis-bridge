namespace Application.Contracts;

public interface IUserService
{
    Task<Result<PagedResponse<IEnumerable<GetAllUserResponse>>>>
        GetUsersAsync(UserFilter filter, CancellationToken token = default);

    Task<Result<GetUserDetailPublicResponse>>
        GetByIdForUser(Guid userId, CancellationToken token = default);

    Task<Result<GetUserDetailPrivateResponse>>
        GetByIdForSelf(CancellationToken token = default);

    Task<Result<UpdateUserResponse>>
        UpdateProfileAsync(UpdateUserProfileRequest request, CancellationToken token = default);
}