namespace API.Controllers.V1;

[Route($"{ApiAddress.Base}/linked-accounts")]
public sealed class WalletLinkedAccountController(IWalletLinkedAccountService service) : V1BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateWalletLinkedAccountRequest request,
        CancellationToken token = default)
        => (await service.CreateAsync(request, token)).ToActionResult();

    [HttpGet("me")]
    public async Task<IActionResult> GetAsync(CancellationToken token = default)
        => (await service.GetAsync(token)).ToActionResult();
}