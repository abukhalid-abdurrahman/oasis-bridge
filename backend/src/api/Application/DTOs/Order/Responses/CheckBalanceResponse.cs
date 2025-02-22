namespace Application.DTOs.Order.Responses;

public record CheckBalanceResponse(
    Guid OrderId,
    string Network,
    string Token,
    decimal Balance,
    decimal RequiredBalance,
    string Status,
    string Message,
    string? TransactionId
);