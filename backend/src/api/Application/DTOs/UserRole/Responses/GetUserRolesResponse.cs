namespace Application.DTOs.UserRole.Responses;

public sealed record GetUserRolesResponse(
    Guid Id,
    Guid UserId,
    Guid RoleId,
    GetUserDetailPublicResponse User,
    GetRoleDetailResponse Role);