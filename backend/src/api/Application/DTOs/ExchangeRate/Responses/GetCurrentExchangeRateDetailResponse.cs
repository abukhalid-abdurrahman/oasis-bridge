namespace Application.DTOs.ExchangeRate.Responses;

public record GetCurrentExchangeRateDetailResponse(
    Guid ExchangeRateId,
    string FromToken,
    string ToToken,
    decimal Rate,
    DateTimeOffset Timestamp,
    string Message
);