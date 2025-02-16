namespace Infrastructure.ImplementationContract;

public sealed class AccountService(
    DataContext dbContext,
    IHttpContextAccessor accessor,
    IEmailService emailService,
    ILogger<AccountService> logger,
    IConfiguration configuration) : IAccountService
{
    public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        bool checkExisting = await dbContext.Users.IgnoreQueryFilters()
            .AnyAsync(x =>
                x.UserName == request.UserName ||
                x.Email == request.EmailAddress ||
                x.PhoneNumber == request.PhoneNumber, token);

        if (checkExisting)
            return Result<RegisterResponse>.Failure(ResultPatternError.AlreadyExist());

        Role? existingRole = await dbContext.Roles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == Roles.User, token);

        if (existingRole is null)
            return Result<RegisterResponse>.Failure(ResultPatternError.NotFound("Role 'User' not found!"));

        User user = request.ToEntity(accessor);

        UserRole userRole = new()
        {
            UserId = user.Id,
            RoleId = existingRole.Id,
            CreatedBy = HttpAccessor.SystemId,
            CreatedByIp = accessor.GetRemoteIpAddress(),
        };

        UserVerificationCode userVerificationCode = new()
        {
            CreatedByIp = user.CreatedByIp,
            Code = VerificationHelper.GenerateVerificationCode(),
            StartTime = DateTimeOffset.UtcNow,
            ExpiryTime = DateTimeOffset.UtcNow.AddMinutes(1),
            UserId = user.Id,
            Type = VerificationCodeType.None,
            CreatedBy = HttpAccessor.SystemId
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

        user.TokenVersion = Guid.NewGuid();
        await dbContext.SaveChangesAsync(token);

        Result<LoginResponse> result = await dbContext.GenerateTokenAsync(user, configuration);

        if (!result.IsSuccess)
        {
            logger.LogError($"Error generating token: {result}");
            return Result<LoginResponse>.Failure(ResultPatternError.InternalServerError("Error generating token!"));
        }

        await Task.Run(async () =>
        {
            string? userAgent = accessor.GetUserAgent();

            string remoteIpAddress = accessor.GetRemoteIpAddress();

            user.TotalLogins++;
            user.LastLoginAt = DateTimeOffset.UtcNow;

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
                CreatedBy = HttpAccessor.SystemId,
                IpAddress = remoteIpAddress
            };
            await dbContext.UserLogins.AddAsync(userLogin, token);
            await dbContext.UserTokens.AddAsync(userToken, token);

            int res = await dbContext.SaveChangesAsync(token);

            if (res != 0)
            {
                BaseResult emailResult = await emailService.SendEmailAsync(user.Email,
                    "Successful Login to OASIS Bridge",
                    "You have successfully logged into your OASIS Bridge account. If this was not you, please contact support immediately.");

                if (!emailResult.IsSuccess)
                    logger.LogError("Failed to send login notification email.");
            }
            else
                logger.LogError($"Failed save information about login. Time: {DateTimeOffset.UtcNow} .");
        }, token);

        return Result<LoginResponse>.Success(result.Value);
    }


    public async Task<BaseResult> LogoutAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        Guid? userId = accessor.GetId();

        if (userId is null)
            return BaseResult.Failure(ResultPatternError.BadRequest("Invalid user ID."));

        User? user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, token);
        if (user is null) return BaseResult.Failure(ResultPatternError.NotFound("User not found."));

        user.TokenVersion = Guid.NewGuid();
        user.UpdatedBy = userId;
        user.UpdatedByIp!.Add(accessor.GetRemoteIpAddress());
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.Version++;

        int res = await dbContext.SaveChangesAsync(token);

        return res != 0
            ? BaseResult.Success()
            : BaseResult.Failure(ResultPatternError.InternalServerError("Could not logout!"));
    }

    public async Task<BaseResult> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        Guid? userId = accessor.GetId();

        if (userId is null)
            return BaseResult.Failure(ResultPatternError.BadRequest("Invalid user ID."));

        User? user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, token);
        if (user is null)
            return BaseResult.Failure(ResultPatternError.NotFound("User not found."));

        string hashedOldPassword = HashingUtility.ComputeSha256Hash(request.OldPassword);
        if (user.PasswordHash != hashedOldPassword)
            return BaseResult.Failure(ResultPatternError.BadRequest("Old password is incorrect."));

        user.PasswordHash = HashingUtility.ComputeSha256Hash(request.NewPassword);
        user.UpdatedBy = userId;
        user.UpdatedByIp!.Add(accessor.GetRemoteIpAddress());
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.Version++;
        user.LastPasswordChangeAt = DateTimeOffset.UtcNow;
        user.TokenVersion = Guid.NewGuid();


        int res = await dbContext.SaveChangesAsync(token);
        if (res == 0)
            return BaseResult.Failure(ResultPatternError.InternalServerError("Could not change password."));

        BaseResult emailResult = await emailService.SendEmailAsync(user.Email,
            "Password Changed Successfully",
            "Your password has been changed successfully. If this was not you, please contact support immediately.");

        if (!emailResult.IsSuccess)
            logger.LogError("Failed to send password change notification email.");

        return BaseResult.Success();
    }


    public async Task<BaseResult> SendEmailConfirmationCodeAsync(SendEmailConfirmationCodeRequest request,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        User? user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == request.Email, token);
        if (user is null)
            return BaseResult.Failure(ResultPatternError.NotFound("User with this email was not found."));

        if(user.EmailConfirmed) 
            return BaseResult.Failure(ResultPatternError.BadRequest("Your email address already confirmed"));

        long verificationCode = VerificationHelper.GenerateVerificationCode();

        UserVerificationCode userVerificationCode = new()
        {
            CreatedByIp = accessor.GetRemoteIpAddress(),
            Code = verificationCode,
            StartTime = DateTimeOffset.UtcNow,
            ExpiryTime = DateTimeOffset.UtcNow.AddMinutes(1),
            UserId = user.Id,
            Type = VerificationCodeType.EmailConfirmation,
            CreatedBy = user.Id,
        };

        await dbContext.UserVerificationCodes.AddAsync(userVerificationCode, token);
        int res = await dbContext.SaveChangesAsync(token);

        if (res == 0)
            return BaseResult.Failure(
                ResultPatternError.InternalServerError("Could not generate email confirmation code."));

        BaseResult emailResult = await emailService.SendEmailAsync(request.Email,
            "Email Confirmation Code",
            $"Your email confirmation code is: {verificationCode}");

        if (!emailResult.IsSuccess)
        {
            logger.LogError("Failed to send email confirmation code to {Email}", request.Email);
            return BaseResult.Failure(
                ResultPatternError.InternalServerError("Failed to send email confirmation code."));
        }

        return BaseResult.Success();
    }


    public async Task<BaseResult> ConfirmEmailAsync(ConfirmEmailCodeRequest request, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        User? user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == request.Email, token);
        if (user is null)
            return BaseResult.Failure(ResultPatternError.NotFound("User with this email was not found."));

        UserVerificationCode? verificationCode = await dbContext.UserVerificationCodes
            .Where(x => x.UserId == user.Id && x.Type == VerificationCodeType.EmailConfirmation)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(token);

        if (verificationCode is null ||
            verificationCode.Code != long.Parse(request.Code) ||
            (verificationCode.ExpiryTime - DateTimeOffset.UtcNow).TotalSeconds >= 60)
            return BaseResult.Failure(ResultPatternError.BadRequest("Invalid or expired reset code."));

        if (verificationCode.Code != long.Parse(request.Code))
            return BaseResult.Failure(ResultPatternError.BadRequest("Invalid verification code."));

        user.EmailConfirmed = true;
        user.UpdatedBy = user.Id;
        user.UpdatedByIp!.Add(accessor.GetRemoteIpAddress());
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.Version++;

        int res = await dbContext.SaveChangesAsync(token);
        if (res == 0)
            return BaseResult.Failure(ResultPatternError.InternalServerError("Could not confirm email."));

        BaseResult emailResult = await emailService.SendEmailAsync(user.Email,
            "Your Email Has Been Confirmed",
            "Congratulations! Your email has been successfully confirmed. You can now use all features of OASIS Bridge.");

        if (!emailResult.IsSuccess)
            logger.LogError("Failed to send email confirmation notification.");

        return BaseResult.Success();
    }


    public async Task<BaseResult> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        User? user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == request.EmailAddress, token);
        if (user is null)
            return BaseResult.Failure(ResultPatternError.NotFound("User with this email was not found."));

        UserVerificationCode resetCode = new()
        {
            UserId = user.Id,
            CreatedByIp = accessor.GetRemoteIpAddress(),
            Code = VerificationHelper.GenerateVerificationCode(),
            StartTime = DateTimeOffset.UtcNow,
            ExpiryTime = DateTimeOffset.UtcNow.AddMinutes(1),
            Type = VerificationCodeType.PasswordReset,
            CreatedBy = user.Id
        };

        await dbContext.UserVerificationCodes.AddAsync(resetCode, token);
        int res = await dbContext.SaveChangesAsync(token);

        if (res == 0)
            return BaseResult.Failure(
                ResultPatternError.InternalServerError("Could not generate password reset code."));

        BaseResult emailResult = await emailService.SendEmailAsync(request.EmailAddress,
            "Password Reset Request",
            $"You have requested to reset your password. Use this code to proceed: {resetCode.Code}. The code is valid for 1 minutes.");

        if (!emailResult.IsSuccess)
            logger.LogError("Failed to send password reset email.");

        return BaseResult.Success();
    }


    public async Task<BaseResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        User? user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == request.EmailAddress, token);
        if (user is null)
            return BaseResult.Failure(ResultPatternError.NotFound("User with this email was not found."));

        UserVerificationCode? resetCode = await dbContext.UserVerificationCodes
            .Where(x => x.UserId == user.Id &&
                        x.Type == VerificationCodeType.PasswordReset &&
                        x.Code == long.Parse(request.ResetCode))
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(token);

        if (resetCode is null)
            return BaseResult.Failure(ResultPatternError.BadRequest("Invalid reset code."));

        if (resetCode.ExpiryTime < DateTimeOffset.UtcNow ||
            (resetCode.ExpiryTime - DateTimeOffset.UtcNow).TotalSeconds >= 60)
            return BaseResult.Failure(ResultPatternError.BadRequest("Reset code has expired."));


        user.PasswordHash = HashingUtility.ComputeSha256Hash(request.NewPassword);
        user.UpdatedBy = user.Id;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.Version++;
        user.UpdatedByIp!.Add(accessor.GetRemoteIpAddress());
        user.TokenVersion = Guid.NewGuid();

        int res = await dbContext.SaveChangesAsync(token);
        if (res == 0)
            return BaseResult.Failure(ResultPatternError.InternalServerError("Could not reset password."));

        BaseResult emailResult = await emailService.SendEmailAsync(request.EmailAddress,
            "Password Successfully Reset",
            "Your password has been successfully reset. If you did not request this change, please contact support immediately.");

        if (!emailResult.IsSuccess)
            logger.LogError($"Failed to send password reset confirmation email to {request.EmailAddress}");

        return BaseResult.Success();
    }


    public async Task<BaseResult> RestoreAccountAsync(RestoreAccountRequest request, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        User? user = await dbContext.Users
            .IgnoreQueryFilters().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == request.Email, token);

        if (user is null)
            return BaseResult.Failure(ResultPatternError.NotFound("User with this email was not found."));

        if (!user.IsDeleted)
            return BaseResult.Failure(ResultPatternError.BadRequest("This account is already active."));

        UserVerificationCode restoreCode = new()
        {
            CreatedByIp = accessor.GetRemoteIpAddress(),
            Code = VerificationHelper.GenerateVerificationCode(),
            StartTime = DateTimeOffset.UtcNow,
            ExpiryTime = DateTimeOffset.UtcNow.AddMinutes(1),
            UserId = user.Id,
            Type = VerificationCodeType.AccountRestore,
            CreatedBy = HttpAccessor.SystemId
        };

        await dbContext.UserVerificationCodes.AddAsync(restoreCode, token);
        int res = await dbContext.SaveChangesAsync(token);

        if (res != 0)
            return BaseResult.Failure(ResultPatternError.InternalServerError("Could not restore code"));

        BaseResult emailResult = await emailService.SendEmailAsync(request.Email,
            "Restore Your OASIS Bridge Account",
            $"Use this verification code to restore your account: {restoreCode.Code}");

        if (!emailResult.IsSuccess)
            logger.LogError("Failed to send restore account email.");

        return BaseResult.Success("Verification code has been sent to your email.");
    }


    public async Task<BaseResult> ConfirmRestoreAccountAsync(
        ConfirmRestoreAccountRequest request, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        User? user = await dbContext.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Email == request.Email, token);

        if (user is null)
            return BaseResult.Failure(
                ResultPatternError.NotFound("User with this email was not found."));

        if (!user.IsDeleted)
            return BaseResult.Failure(ResultPatternError.BadRequest("This account is already active."));

        UserVerificationCode? restoreCode = await dbContext.UserVerificationCodes
            .Where(x =>
                x.UserId == user.Id &&
                x.Code == long.Parse(request.Code) &&
                x.Type == VerificationCodeType.AccountRestore)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(token);

        if (restoreCode is null)
            return BaseResult.Failure(
                ResultPatternError.BadRequest("Invalid restore code."));

        double difference = (DateTimeOffset.UtcNow - user.DeletedAt!.Value).TotalDays;
        if (difference > 30)
            return BaseResult.Failure(ResultPatternError.BadRequest(
                "The restore code has expired. Please contact technical support to restore your account."));

        if (restoreCode.ExpiryTime < DateTimeOffset.UtcNow ||
            (restoreCode.ExpiryTime - DateTimeOffset.UtcNow).TotalSeconds >= 60)
            return BaseResult.Failure(ResultPatternError.BadRequest("Reset code has expired."));


        user.DeletedAt = null;
        user.DeletedBy = null;
        user.DeletedByIp = null;
        user.IsDeleted = false;
        user.UpdatedBy = user.Id;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.Version++;
        user.UpdatedByIp!.Add(accessor.GetRemoteIpAddress());
        user.TokenVersion = Guid.NewGuid();

        int res = await dbContext.SaveChangesAsync(token);

        if (res == 0)
            return BaseResult.Failure(
                ResultPatternError.InternalServerError("Could not restore account."));

        BaseResult emailResult = await emailService.SendEmailAsync(request.Email,
            "Account Restored",
            "Your account has been successfully restored. If you did not request this, please contact support immediately.");

        if (!emailResult.IsSuccess)
            logger.LogError("Failed to send account restored email.");

        return BaseResult.Success("Account successfully restored.");
    }


    public async Task<BaseResult> DeleteAccountAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        Guid userId = accessor.GetId() ?? HttpAccessor.SystemId;
        string remoteIpAddress = accessor.GetRemoteIpAddress();

        User? user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, token);

        if (user is null)
            return BaseResult.Failure(ResultPatternError.NotFound("User not found."));

        if (user.IsDeleted)
            return BaseResult.Failure(ResultPatternError.BadRequest("Account is already deleted."));

        user.DeletedAt = DateTimeOffset.UtcNow;
        user.DeletedByIp = remoteIpAddress;
        user.DeletedBy = userId;
        user.IsDeleted = true;
        user.UpdatedBy = userId;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.UpdatedByIp!.Add(remoteIpAddress);
        user.Version++;
        user.TokenVersion = Guid.NewGuid();


        int res = await dbContext.SaveChangesAsync(token);
        if (res == 0)
            return BaseResult.Failure(ResultPatternError.InternalServerError("Could not delete account."));

        BaseResult emailResult = await emailService.SendEmailAsync(user.Email,
            "Account Deleted",
            "Your account has been successfully deleted. If you did not request this, please contact support immediately. " +
            "You can restore your account within the next 30 days by using the restore feature in your account settings.");

        if (!emailResult.IsSuccess)
            logger.LogError("Failed to send account deletion email.");

        return BaseResult.Success("Account successfully deleted.");
    }
}