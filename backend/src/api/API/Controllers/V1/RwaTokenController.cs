namespace API.Controllers.V1;

[Route($"{ApiAddress.Base}/rwa")]
public class RwaTokenController(IRwaTokenService service) : V1BaseController
{
    [HttpPost("tokenize")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateRwaTokenRequest request, CancellationToken token)
        => (await service.CreateAsync(request, token)).ToActionResult();

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetailAsync(Guid id, CancellationToken token)
        => (await service.GetDetailAsync(id, token)).ToActionResult();

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] RwaTokenFilter filter, CancellationToken token)
        => (await service.GetAllAsync(filter, token)).ToActionResult();

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Test(
        [FromServices] ISolanaNftManager solanaNftManager,
        [FromServices] DataContext dbContext,
        [FromServices] IHttpContextAccessor accessor,
        Guid id)
    {
        NftBurnRequest? result = await (from u in dbContext.Users
                join va in dbContext.VirtualAccounts on u.Id equals va.UserId
                join n in dbContext.Networks on va.NetworkId equals n.Id
                join rw in dbContext.RwaTokens on va.Id equals rw.VirtualAccountId
                where u.Id == accessor.GetId() && n.Name == "Solana" && rw.Id == id
                select new NftBurnRequest()
                {
                    MintAddress = rw.MintAccount,
                    OwnerPrivateKey = va.PrivateKey,
                    OwnerPublicKey = va.PublicKey,
                    OwnerSeedPhrase = va.SeedPhrase
                }
            ).FirstOrDefaultAsync();
        if (result is null)
        {
            return BadRequest(Messages.WalletNotFound);
        }

        await solanaNftManager.BurnAsync(result);
        return Ok();
    }
}