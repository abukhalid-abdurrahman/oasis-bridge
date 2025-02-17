namespace Application.DTOs.ExchangeRate.Responses;

public record GetExchangeRateResponse(
    Guid Id,
    Guid FromTokenId,
    GetNetworkTokenDetailResponse FromNetworkToken,
    Guid ToTokenId,
    GetNetworkTokenDetailResponse ToNetworkToken,
    decimal Rate,
    DateTimeOffset CreatedAt
);