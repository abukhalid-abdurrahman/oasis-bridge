namespace Application.DTOs.Role.Requests;

public sealed record CreateRoleRequest(
    string RoleName,
    string RoleKey,
    string? Description);