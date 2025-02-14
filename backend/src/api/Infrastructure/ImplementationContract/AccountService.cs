using Application.Extensions.Mappers;
using BuildingBlocks.Extensions.Smtp;
using Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.ImplementationContract;

public sealed class AccountService(
    DataContext dbContext,
    HttpContextAccessor accessor,
    IEmailService emailService,
    ILogger<AccountService> logger,
    IConfiguration configuration) : IAccountService
{
    private static readonly Guid SystemId = new("11111111-1111-1111-1111-111111111111");

    public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        bool checkExisting = await dbContext.Users.AnyAsync(x =>
            x.UserName == request.UserName ||
            x.Email == request.EmailAddress ||
            x.PhoneNumber == request.PhoneNumber, token);

        if (checkExisting)
            return Result<RegisterResponse>.Failure(ResultPatternError.AlreadyExist());

        Role? existingRole = await dbContext.Roles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == Roles.User, token);

        if (existingRole is null)
            return Result<RegisterResponse>.Failure(ResultPatternError.NotFound("Role 'User' not found!"));

        User user = request.ToEntity(accessor, SystemId);

        UserRole userRole = new()
        {
            UserId = user.Id,
            RoleId = existingRole.Id,
            CreatedBy = SystemId,
            CreatedByIp = user.CreatedByIp,
        };

        UserVerificationCode userVerificationCode = new()
        {
            CreatedByIp = user.CreatedByIp,
            Code = VerificationHelper.GenerateVerificationCode(),
            StartTime = DateTimeOffset.UtcNow,
            ExpiryTime = DateTimeOffset.UtcNow.AddMinutes(1),
            UserId = user.Id,
            Type = VerificationCodeType.None,
            CreatedBy = SystemId
        };

        await dbContext.Users.AddAsync(user, token);
        await dbContext.UserRoles.AddAsync(userRole, token);
        await dbContext.UserVerificationCodes.AddAsync(userVerificationCode, token);

        int res = await dbContext.SaveChangesAsync(token);

        if (res == 0)
            return Result<RegisterResponse>.Failure(ResultPatternError.InternalServerError("Could not register user."));

        BaseResult emailResult = await emailService.SendEmailAsync(request.EmailAddress,
            "Welcome to OASIS Bridge!",
            "You have successfully registered on OASIS Bridge. If you did not create this account, please contact support.");

        if (!emailResult.IsSuccess)
            logger.LogError("Failed to send verification email.");

        return Result<RegisterResponse>.Success(new(user.Id));
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        User? user = await dbContext.Users.FirstOrDefaultAsync(x
            => (x.UserName == request.Login ||
                x.PhoneNumber == request.Login ||
                x.Email == request.Login) &&
               x.PasswordHash == HashingUtility.ComputeSha256Hash(request.Password), token);

        if (user is null)
            return Result<LoginResponse>.Failure(ResultPatternError.BadRequest("Incorrect login or password"));

        Result<LoginResponse> result = await dbContext.GenerateTokenAsync(user, configuration);

        if (!result.IsSuccess)
        {
            logger.LogError($"Error generating token: {result}");
            return Result<LoginResponse>.Failure(ResultPatternError.InternalServerError("Error generating token!"));
        }

        await Task.Run(async () =>
        {
            string? userAgent = accessor.HttpContext?.Request.Headers.UserAgent;

            string? remoteIpAddress = accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

            UserToken userToken = new()
            {
                UserId = user.Id,
                UserAgent = userAgent,
                Token = result.Value!.Token,
                IpAddress = remoteIpAddress,
                TokenType = TokenType.AccessToken,
                Expiration = result.Value!.ExpiresAt,
            };

            UserLogin userLogin = new()
            {
                UserId = user.Id,
                UserAgent = userAgent,
                Successful = true,
                CreatedByIp = remoteIpAddress,
                CreatedBy = SystemId,
                IpAddress = remoteIpAddress
            };
            await dbContext.UserLogins.AddAsync(userLogin, token);
            await dbContext.UserTokens.AddAsync(userToken, token);

            await dbContext.SaveChangesAsync(token);

            BaseResult emailResult = await emailService.SendEmailAsync(user.Email,
                "Successful Login to OASIS Bridge",
                "You have successfully logged into your OASIS Bridge account. If this was not you, please contact support immediately.");

            if (!emailResult.IsSuccess)
                logger.LogError("Failed to send login notification email.");
        }, token);

        return Result<LoginResponse>.Success(result.Value);
    }


    public async Task<BaseResult> LogoutAsync()
    {
        Guid userId = new(accessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.Id)
            ?.ToString() ?? "");
        if (userId.ToString().Length > 10) return BaseResult.Failure(ResultPatternError.BadRequest("Invalid user id."));
        
    }

    public async Task<BaseResult> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResult> SendEmailConfirmationCodeAsync(SendEmailConfirmationCodeRequest request,
        CancellationToken token = default)
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

    public async Task<Result<ConfirmRestoreAccountResponse>> ConfirmRestoreAccountAsync(
        ConfirmRestoreAccountRequest request, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResult> DeleteAccountAsync(Guid id, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}