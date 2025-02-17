namespace Application.Extensions.Mappers;

public static class ExchangeRateMapper
{
    public static GetExchangeRateResponse ToRead(this ExchangeRate rate)
        => new(
            rate.Id,
            rate.FromToken.NetworkId,
            rate.FromToken.ToReadDetail(),
            rate.ToToken.NetworkId,
            rate.ToToken.ToReadDetail(),
            rate.Rate,
            rate.CreatedAt);

    public static GetExchangeRateDetailResponse ToReadDetail(this ExchangeRate rate)
        => new(
            rate.Id,
            rate.FromToken.NetworkId,
            rate.FromToken.ToReadDetail(),
            rate.ToToken.NetworkId,
            rate.ToToken.ToReadDetail(),
            rate.Rate,
            rate.CreatedAt);
}