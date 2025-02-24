namespace API.Controllers.V1;

[Route($"{ApiAddress.Base}/orders")]
[Authorize]
public sealed class OrderController(IOrderService orderService) : V1BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateOrderAsync([FromBody] CreateOrderRequest request,
        CancellationToken token = default)
        => (await orderService.CreateOrderAsync(request, token)).ToActionResult();

    [HttpGet("{orderId:guid}/check-balance")]
    public async Task<IActionResult> CheckOrderBalance([FromRoute] Guid orderId, CancellationToken token = default)
        => (await orderService.CheckBalanceAsync(orderId, token)).ToActionResult();
}