namespace API.Controllers.V1;

[Route($"{ApiAddress.Base}")]
[AllowAnonymous]
public sealed class ExchangeRateController(IExchangeRateService exchangeRateService) : V1BaseController
{
    [HttpGet("history")]
    public async Task<IActionResult> GetExchangeRatesAsync([FromQuery] ExchangeRateFilter filter,
        CancellationToken cancellationToken)
        => (await exchangeRateService.GetExchangeRatesAsync(filter, cancellationToken)).ToActionResult();

    [HttpGet("history/{exchangeRateId:guid}")]
    public async Task<IActionResult> GetExchangeRateDetailAsync(Guid exchangeRateId,
        CancellationToken cancellationToken)
        => (await exchangeRateService.GetExchangeRateDetailAsync(exchangeRateId, cancellationToken)).ToActionResult();

    [HttpGet("exchange-rate")]
    public async Task<IActionResult> ExchangeRateAsync([FromQuery] ExchangeRateRequest request, CancellationToken token)
        => (await exchangeRateService.GetCurrentExchangeRateDetailAsync(request, token)).ToActionResult();
}