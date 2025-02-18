namespace Application.DTOs.Order.Requests;

public record CreateOrderRequest(
    Guid UserId,
    string FromNetwork,
    string FromToken,
    string ToNetwork,
    string ToToken,
    decimal Amount,
    string DestinationAddress
);