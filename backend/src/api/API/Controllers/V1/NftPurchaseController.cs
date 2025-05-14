namespace API.Controllers.V1;

[Route($"{ApiAddresses.Base}/nft-purchase")]
public sealed class NftPurchaseController(INftPurchaseService nftPurchaseService) : V1BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] Guid rwaId)
        => (await nftPurchaseService.CreateAsync(rwaId)).ToActionResult();

    [HttpPost("send")]
    public async Task<IActionResult> SendAsync([FromBody] string transactionHash)
        => (await nftPurchaseService.SendAsync(transactionHash)).ToActionResult();
}