namespace Application.DTOs.Order.Responses;

public record CreateOrderResponse(
    Guid OrderId,
    string? Message
);