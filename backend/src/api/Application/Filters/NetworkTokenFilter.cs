namespace Application.Filters;

public sealed record NetworkTokenFilter(
    string? Symbol,
    string? Description,
    Guid? NetworkId) : BaseFilter;