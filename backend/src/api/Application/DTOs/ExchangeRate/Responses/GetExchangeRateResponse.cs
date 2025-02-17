namespace Application.DTOs.ExchangeRate.Responses;

public record GetExchangeRateResponse(
    Guid Id,
    Guid FromNetworkId,
    GetNetworkDetailResponse FromNetworkToken,
    Guid ToNetworkId,
    GetNetworkDetailResponse ToNetworkToken,
    decimal Rate,
    DateTimeOffset CreatedAt
);