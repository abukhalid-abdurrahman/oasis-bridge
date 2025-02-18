namespace API.Controllers.V1;

[Route($"{ApiAddress.Base}/auth")]
public sealed class IdentityController(IIdentityService identityService) : V1BaseController
{
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
        => (await identityService.RegisterAsync(request, cancellationToken)).ToActionResult();

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
        => (await identityService.LoginAsync(request, cancellationToken)).ToActionResult();

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken)
        => (await identityService.LogoutAsync(cancellationToken)).ToActionResult();

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
        => (await identityService.ChangePasswordAsync(request, cancellationToken)).ToActionResult();

    [HttpPost("email/confirm/code")]
    [Authorize]
    public async Task<IActionResult> SendEmailConfirmCodeAsync([FromBody] SendEmailConfirmationCodeRequest request,
        CancellationToken cancellationToken)
        => (await identityService.SendEmailConfirmationCodeAsync(request, cancellationToken)).ToActionResult();

    [HttpPost("email/confirm")]
    [Authorize]
    public async Task<IActionResult> EmailConfirmAsync([FromBody] ConfirmEmailCodeRequest request,
        CancellationToken cancellationToken)
        => (await identityService.ConfirmEmailAsync(request, cancellationToken)).ToActionResult();

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
        => (await identityService.ForgotPasswordAsync(request, cancellationToken)).ToActionResult();

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
        => (await identityService.ResetPasswordAsync(request, cancellationToken)).ToActionResult();

    [HttpPost("restore")]
    public async Task<IActionResult> RestoreAccountAsync([FromBody] RestoreAccountRequest request,
        CancellationToken cancellationToken)
        => (await identityService.RestoreAccountAsync(request, cancellationToken)).ToActionResult();

    [HttpPost("restore/confirm")]
    public async Task<IActionResult> ConfirmRestoreAccountAsync([FromBody] ConfirmRestoreAccountRequest request,
        CancellationToken cancellationToken)
        => (await identityService.ConfirmRestoreAccountAsync(request, cancellationToken)).ToActionResult();

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteAccountAsync(CancellationToken cancellationToken)
        => (await identityService.DeleteAccountAsync(cancellationToken)).ToActionResult();
}