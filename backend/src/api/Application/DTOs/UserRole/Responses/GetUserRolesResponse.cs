namespace Application.DTOs.UserRole.Responses;

public sealed record GetUserRolesResponse(
    Guid UserId,
    Guid RoleId,
    GetUserDetailPublicResponse User,
    GetRoleDetailResponse Role);