namespace Application.DTOs.UserRole.Requests;

public sealed record CreateUserRoleRequest(
    Guid UserId,
    Guid RoleId) : BaseUserRoleRequest(UserId, RoleId);