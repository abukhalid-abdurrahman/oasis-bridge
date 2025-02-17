namespace Application.Filters;

public sealed record NetworkFilter(
    string? Name,
    string? Description,
    NetworkType? NetworkType) : BaseFilter;