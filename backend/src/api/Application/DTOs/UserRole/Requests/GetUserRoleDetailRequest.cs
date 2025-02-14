namespace Application.DTOs.UserRole.Requests;

public sealed record GetUserRoleDetailRequest(
    Guid UserId,
    Guid RoleId) :BaseUserRoleRequest(UserId, RoleId);