namespace Application.DTOs.UserRole.Requests;

public sealed record DeleteUserRoleRequest(
    Guid UserId,
    Guid RoleId) : BaseUserRoleRequest(UserId, RoleId);