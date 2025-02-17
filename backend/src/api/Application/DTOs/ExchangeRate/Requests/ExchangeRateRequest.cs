namespace Application.DTOs.ExchangeRate.Requests;

public record ExchangeRateRequest(
    string FromToken,
    string ToToken);