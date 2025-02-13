namespace Application.Filters;

public sealed record RoleFilter(
    string? Name,
    string? Keyword,
    string? Description):BaseFilter;