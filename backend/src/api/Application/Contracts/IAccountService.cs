namespace Application.Contracts;

public interface IAccountService
{
    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request,
        CancellationToken token = default);

    Task<Result<LoginResponse>> LoginAsync(LoginRequest request,
        CancellationToken token = default);

    Task<BaseResult> LogoutAsync(CancellationToken token = default);

    Task<BaseResult> ChangePasswordAsync(ChangePasswordRequest request,
        CancellationToken token = default);

    Task<BaseResult> SendEmailConfirmationCodeAsync(SendEmailConfirmationCodeRequest request,
        CancellationToken token = default);

    Task<BaseResult> ConfirmEmailAsync(ConfirmEmailCodeRequest request,
        CancellationToken token = default);

    Task<BaseResult> ForgotPasswordAsync(ForgotPasswordRequest request,
        CancellationToken token = default);

    Task<BaseResult> ResetPasswordAsync(ResetPasswordRequest request,
        CancellationToken token = default);

    Task<BaseResult> RestoreAccountAsync(RestoreAccountRequest request,
        CancellationToken token = default);

    Task<BaseResult> ConfirmRestoreAccountAsync(ConfirmRestoreAccountRequest request,
        CancellationToken token = default);

    Task<BaseResult> DeleteAccountAsync(CancellationToken token = default);
}