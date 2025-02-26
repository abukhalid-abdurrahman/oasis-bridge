namespace API.Controllers.V1;

[Route($"{ApiAddress.Base}/network-tokens")]
[Authorize]
public sealed class NetworkTokenController(INetworkTokenService networkTokenService) : V1BaseController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetNetworkTokensAsync([FromQuery] NetworkTokenFilter filter,
        CancellationToken cancellationToken)
        => (await networkTokenService.GetNetworkTokensAsync(filter, cancellationToken)).ToActionResult();

    [HttpGet("{networkTokenId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetNetworkTokenDetailAsync(Guid networkTokenId,
        CancellationToken cancellationToken)
        => (await networkTokenService.GetNetworkTokenDetailAsync(networkTokenId, cancellationToken)).ToActionResult();

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CreateNetworkTokenAsync([FromBody] CreateNetworkTokenRequest request,
        CancellationToken cancellationToken)
        => (await networkTokenService.CreateNetworkTokenAsync(request, cancellationToken)).ToActionResult();

    [HttpPut("{networkTokenId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> UpdateNetworkTokenAsync(Guid networkTokenId,
        [FromBody] UpdateNetworkTokenRequest request, CancellationToken cancellationToken)
        => (await networkTokenService.UpdateNetworkTokenAsync(networkTokenId, request, cancellationToken))
            .ToActionResult();

    [HttpDelete("{networkTokenId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteNetworkTokenAsync(Guid networkTokenId, CancellationToken cancellationToken)
        => (await networkTokenService.DeleteNetworkTokenAsync(networkTokenId, cancellationToken)).ToActionResult();
}