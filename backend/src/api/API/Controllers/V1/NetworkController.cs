namespace API.Controllers.V1;

[Route($"{ApiAddress.Base}/networks")]
[Authorize]
public sealed class NetworkController(INetworkService networkService) : V1BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetNetworksAsync([FromQuery] NetworkFilter filter,
        CancellationToken cancellationToken)
        => (await networkService.GetNetworksAsync(filter, cancellationToken)).ToActionResult();

    [HttpGet("{networkId:guid}")]
    public async Task<IActionResult> GetNetworkDetailAsync(Guid networkId, CancellationToken cancellationToken)
        => (await networkService.GetNetworkDetailAsync(networkId, cancellationToken)).ToActionResult();

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CreateNetworkAsync([FromBody] CreateNetworkRequest request,
        CancellationToken cancellationToken)
        => (await networkService.CreateNetworkAsync(request, cancellationToken)).ToActionResult();

    [HttpPut("{networkId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> UpdateNetworkAsync(Guid networkId, [FromBody] UpdateNetworkRequest request,
        CancellationToken cancellationToken)
        => (await networkService.UpdateNetworkAsync(networkId, request, cancellationToken)).ToActionResult();

    [HttpDelete("{networkId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteNetworkAsync(Guid networkId, CancellationToken cancellationToken)
        => (await networkService.DeleteNetworkAsync(networkId, cancellationToken)).ToActionResult();
}