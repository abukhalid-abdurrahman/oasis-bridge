namespace Application.Filters;

public sealed record UserRoleFilter(
    string? FirstName,
    string? LastName,
    string? Email,
    string? PhoneNumber,
    string? UserName,
    string? RoleName,
    string? RoleKeyword,
    string? RoleDescription):BaseFilter;