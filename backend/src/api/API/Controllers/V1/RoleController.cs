namespace API.Controllers.V1;

[Route($"{ApiAddress.Base}/roles")]
[Authorize(Roles = Roles.Admin)]
public sealed class RoleController(IRoleService roleService) : V1BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetRolesAsync([FromQuery] RoleFilter filter, CancellationToken cancellationToken)
        => (await roleService.GetRolesAsync(filter, cancellationToken)).ToActionResult();

    [HttpGet("{roleId:guid}")]
    public async Task<IActionResult> GetRoleDetailAsync(Guid roleId, CancellationToken cancellationToken)
        => (await roleService.GetRoleDetailAsync(roleId, cancellationToken)).ToActionResult();

    [HttpPost]
    public async Task<IActionResult> CreateRoleAsync([FromBody]CreateRoleRequest request, CancellationToken cancellationToken)
        => (await roleService.CreateRoleAsync(request, cancellationToken)).ToActionResult();

    [HttpPut("{roleId:guid}")]
    public async Task<IActionResult> UpdateRoleAsync(Guid roleId, [FromBody]UpdateRoleRequest request,
        CancellationToken cancellationToken)
        => (await roleService.UpdateRoleAsync(roleId, request, cancellationToken)).ToActionResult();

    [HttpDelete("{roleId:guid}")]
    public async Task<IActionResult> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken)
        => (await roleService.DeleteRoleAsync(roleId, cancellationToken)).ToActionResult();
}