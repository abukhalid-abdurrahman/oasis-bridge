namespace Application.Contracts;

public interface IOrderService
{
    Task<Result<CreateOrderResponse>> CreateOrderAsync(CreateOrderRequest request, CancellationToken token = default);
    Task<Result<CheckBalanceResponse>> CheckBalanceAsync(Guid orderId, CancellationToken token = default);
}