namespace API.Controllers.V1;

[Route($"{ApiAddresses.Base}/nft-purchase")]
public sealed class NftPurchaseController:V1BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateAsync()
}