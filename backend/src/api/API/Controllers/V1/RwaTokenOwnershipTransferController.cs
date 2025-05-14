namespace API.Controllers.V1;

[Route($"{ApiAddresses.Base}/nft-purchase-ownership-histories")]
public sealed class RwaTokenOwnershipTransferController(
    IRwaTokenOwnershipTransferService service) : V1BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken token)
        => (await service.GetAllAsync(token)).ToActionResult();
}