namespace Application.DTOs.Role.Responses;

public record GetRoleDetailResponse(
    Guid Id,
    string RoleName,
    string RoleKey,
    string? Description);