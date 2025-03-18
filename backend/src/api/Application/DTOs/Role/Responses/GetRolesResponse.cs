namespace Application.DTOs.Role.Responses;

public record GetRolesResponse(
    Guid Id,
    string RoleName,
    string RoleKey,
    string? Description);