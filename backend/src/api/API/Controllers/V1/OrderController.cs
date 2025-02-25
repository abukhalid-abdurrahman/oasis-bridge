namespace API.Controllers.V1;

[Route($"{ApiAddress.Base}/orders")]
[Authorize]
public sealed class OrderController(IOrderService orderService) : V1BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateOrderAsync([FromBody] CreateOrderRequest request,
        CancellationToken token = default)
    {
        Result<CreateOrderResponse> result = await orderService.CreateOrderAsync(request, token);
        return result.IsSuccess
            ? Ok(ApiResponse<CreateOrderResponse>.Success(result.Value))
            : Ok(ApiResponse<CreateOrderResponse>.Fail(result.Error));
    }

    [HttpGet("{orderId:guid}/check-balance")]
    public async Task<IActionResult> CheckOrderBalance([FromRoute] Guid orderId, CancellationToken token = default)
    {
        Result<CheckBalanceResponse> result = await orderService.CheckBalanceAsync(orderId, token);
        return result.IsSuccess
            ? Ok(ApiResponse<CheckBalanceResponse>.Success(result.Value))
            : Ok(ApiResponse<CheckBalanceResponse>.Fail(result.Error));
    }
}