namespace API.Controllers.V1;

[Route($"{ApiAddress.Base}/user-roles")]
public sealed class UserRoleController(IUserRoleService userRoleService) : V1BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetUserRolesAsync([FromQuery] UserRoleFilter filter,
        CancellationToken cancellationToken)
        => (await userRoleService.GetUserRolesAsync(filter, cancellationToken)).ToActionResult();

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserRoleDetailAsync(Guid id, CancellationToken cancellationToken)
        => (await userRoleService.GetUserRoleDetailAsync(id, cancellationToken)).ToActionResult();

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CreateUserRoleAsync([FromBody] CreateUserRoleRequest request,
        CancellationToken cancellationToken)
        => (await userRoleService.CreateUserRoleAsync(request, cancellationToken)).ToActionResult();

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> UpdateUserRoleAsync(Guid id, [FromBody] UpdateUserRoleRequest request,
        CancellationToken cancellationToken)
        => (await userRoleService.UpdateUserRoleAsync(id, request, cancellationToken)).ToActionResult();

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteUserRoleAsync(Guid id, CancellationToken cancellationToken)
        => (await userRoleService.DeleteUserRoleAsync(id, cancellationToken)).ToActionResult();
}