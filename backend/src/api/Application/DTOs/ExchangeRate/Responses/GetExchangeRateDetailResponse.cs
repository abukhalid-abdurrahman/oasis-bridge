namespace Application.DTOs.ExchangeRate.Responses;

public record GetExchangeRateDetailResponse(
    Guid Id,
    Guid FromTokenId,
    GetNetworkTokenDetailResponse FromNetworkToken,
    Guid ToTokenId,
    GetNetworkTokenDetailResponse ToNetworkToken,
    decimal Rate,
    DateTimeOffset CreatedAt
);