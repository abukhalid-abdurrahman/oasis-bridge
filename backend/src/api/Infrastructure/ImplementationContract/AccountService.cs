using Application.DTOs.Account.Requests;
using Application.DTOs.Account.Responses;

namespace Infrastructure.ImplementationContract;

public sealed class AccountService : IAccountService
{
    public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResult> LogoutAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResult> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResult> SendEmailConfirmationCodeAsync(SendEmailConfirmationCodeRequest request, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResult> ConfirmEmailAsync(ConfirmEmailCodeRequest request, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResult> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResult> RestoreAccountAsync(RestoreAccountRequest request, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<ConfirmRestoreAccountResponse>> ConfirmRestoreAccountAsync(ConfirmRestoreAccountRequest request, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResult> DeleteAccountAsync(Guid id, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}