namespace Application.DTOs.UserRole.Responses;

public sealed record GetUserRoleDetailResponse(
    Guid UserId,
    Guid RoleId,
    GetUserDetailPublicResponse User,
    GetRoleDetailResponse Role);