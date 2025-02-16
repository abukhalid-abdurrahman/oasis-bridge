namespace API.Controllers.V1;

[Route("users")]
[Authorize]
public sealed class UserController(IUserService userService) : V1BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetUsersAsync([FromQuery] UserFilter filter, CancellationToken cancellationToken)
        => (await userService.GetUsersAsync(filter, cancellationToken)).ToActionResult();

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
        => (await userService.GetByIdForUser(userId, cancellationToken)).ToActionResult();

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfileAsync(CancellationToken cancellationToken)
        => (await userService.GetByIdForSelf(cancellationToken)).ToActionResult();

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfileAsync([FromBody]UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
        => (await userService.UpdateProfileAsync(request, cancellationToken)).ToActionResult();
}