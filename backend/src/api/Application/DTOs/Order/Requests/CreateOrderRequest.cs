namespace Application.DTOs.Order.Requests;

public record CreateOrderRequest(
    string FromNetwork,
    string FromToken,
    string ToNetwork,
    string ToToken,
    decimal Amount,
    string DestinationAddress
);