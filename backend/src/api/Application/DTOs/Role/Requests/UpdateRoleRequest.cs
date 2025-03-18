namespace Application.DTOs.Role.Requests;

public sealed record UpdateRoleRequest(
    string RoleName,
    string RoleKey,
    string? Description);