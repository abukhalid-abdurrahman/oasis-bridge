namespace Application.Filters;

public sealed record ExchangeRateFilter(
    Guid? FromTokenId,
    Guid? ToTokenId,
    decimal? Rate,
    DateTimeOffset? Date) : BaseFilter;