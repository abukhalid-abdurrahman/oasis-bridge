namespace API.Controllers.V1;

[Route("accounts")]
public sealed class AccountController(IAccountService accountService) : V1BaseController
{
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
        => (await accountService.RegisterAsync(request, cancellationToken)).ToActionResult();

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
        => (await accountService.LoginAsync(request, cancellationToken)).ToActionResult();

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken)
        => (await accountService.LogoutAsync(cancellationToken)).ToActionResult();

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
        => (await accountService.ChangePasswordAsync(request, cancellationToken)).ToActionResult();

    [HttpPost("email/confirm/code")]
    [Authorize]
    public async Task<IActionResult> SendEmailConfirmCodeAsync([FromBody] SendEmailConfirmationCodeRequest request,
        CancellationToken cancellationToken)
        => (await accountService.SendEmailConfirmationCodeAsync(request, cancellationToken)).ToActionResult();

    [HttpPost("email/confirm")]
    [Authorize]
    public async Task<IActionResult> EmailConfirmAsync([FromBody] ConfirmEmailCodeRequest request,
        CancellationToken cancellationToken)
        => (await accountService.ConfirmEmailAsync(request, cancellationToken)).ToActionResult();

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
        => (await accountService.ForgotPasswordAsync(request, cancellationToken)).ToActionResult();

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
        => (await accountService.ResetPasswordAsync(request, cancellationToken)).ToActionResult();

    [HttpPost("restore")]
    public async Task<IActionResult> RestoreAccountAsync([FromBody] RestoreAccountRequest request,
        CancellationToken cancellationToken)
        => (await accountService.RestoreAccountAsync(request, cancellationToken)).ToActionResult();

    [HttpPost("restore/confirm")]
    public async Task<IActionResult> ConfirmRestoreAccountAsync([FromBody] ConfirmRestoreAccountRequest request,
        CancellationToken cancellationToken)
        => (await accountService.ConfirmRestoreAccountAsync(request, cancellationToken)).ToActionResult();

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteAccountAsync(CancellationToken cancellationToken)
        => (await accountService.DeleteAccountAsync(cancellationToken)).ToActionResult();
}