namespace API.Controllers.V1;

[Route($"{ApiAddress.Base}")]
[AllowAnonymous]
public sealed class HealthController : V1BaseController
{
    [HttpGet("ping")]
    public Task<IActionResult> Ping() => Task.FromResult<IActionResult>(result: Ok("pong"));
}