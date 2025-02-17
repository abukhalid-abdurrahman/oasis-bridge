namespace API.Controllers.V1;

[Route($"{ApiAddress.Base}/exchange-rates")]
[Authorize]
public sealed class ExchangeRateController(IExchangeRateService exchangeRateService) : V1BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetExchangeRatesAsync([FromQuery] ExchangeRateFilter filter,
        CancellationToken cancellationToken)
        => (await exchangeRateService.GetExchangeRatesAsync(filter, cancellationToken)).ToActionResult();

    [HttpGet("{exchangeRateId:guid}")]
    public async Task<IActionResult> GetExchangeRateDetailAsync(Guid exchangeRateId,
        CancellationToken cancellationToken)
        => (await exchangeRateService.GetExchangeRateDetailAsync(exchangeRateId, cancellationToken)).ToActionResult();
}