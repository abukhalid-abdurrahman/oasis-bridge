namespace Application.DTOs.UserRole.Requests;

public sealed record UpdateUserRoleRequest(
    Guid UserId,
    Guid RoleId);