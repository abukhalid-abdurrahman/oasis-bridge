using Role = Domain.Entities.Role;

namespace Infrastructure.ImplementationContract;

public sealed class IdentityService(
    DataContext dbContext,
    IHttpContextAccessor accessor,
    IEmailService emailService,
    ILogger<IdentityService> logger,
    IConfiguration configuration) : IIdentityService
{
  
    public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        logger.LogInformation("Starting RegisterAsync at {Time}", DateTimeOffset.UtcNow);

        // Check if a user already exists with the provided username or email.
        logger.LogInformation("Checking for existing user with username {UserName} or email {Email}", request.UserName, request.EmailAddress);
        bool checkExisting = await dbContext.Users.IgnoreQueryFilters()
            .AnyAsync(x => x.UserName == request.UserName || x.Email == request.EmailAddress, token);
        if (checkExisting)
        {
            logger.LogWarning("User already exists with the provided username or email.");
            return Result<RegisterResponse>.Failure(ResultPatternError.AlreadyExist());
        }

        // Retrieve the "User" role from the database.
        logger.LogInformation("Retrieving the 'User' role from the database.");
        Role? existingRole = await dbContext.Roles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == Roles.User, token);
        if (existingRole is null)
        {
            logger.LogError("Role 'User' not found!");
            return Result<RegisterResponse>.Failure(ResultPatternError.NotFound("Role 'User' not found!"));
        }

        // Map the request to a User entity.
        logger.LogInformation("Mapping register request to User entity.");
        User user = request.ToEntity(accessor);

        // Create a new UserRole record.
        logger.LogInformation("Creating UserRole entity.");
        UserRole userRole = new()
        {
            UserId = user.Id,
            RoleId = existingRole.Id,
            CreatedBy = HttpAccessor.SystemId,
            CreatedByIp = accessor.GetRemoteIpAddress(),
        };

        // Generate a verification code for the user.
        logger.LogInformation("Generating verification code for the new user.");
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

        // Add user, user role, and verification code to the database.
        logger.LogInformation("Adding new user, user role, and verification code to the database.");
        await dbContext.Users.AddAsync(user, token);
        await dbContext.UserRoles.AddAsync(userRole, token);
        await dbContext.UserVerificationCodes.AddAsync(userVerificationCode, token);

        int res = await dbContext.SaveChangesAsync(token);
        if (res == 0)
        {
            logger.LogError("Failed to register user: Database save returned 0.");
            return Result<RegisterResponse>.Failure(ResultPatternError.InternalServerError("Could not register user."));
        }

        logger.LogInformation("User registered successfully with UserID: {UserId}", user.Id);
        logger.LogInformation("Finishing RegisterAsync at {Time}", DateTimeOffset.UtcNow);
        return Result<RegisterResponse>.Success(new RegisterResponse(user.Id));
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        logger.LogInformation("Starting LoginAsync at {Time}", DateTimeOffset.UtcNow);

        // Retrieve user based on email and hashed password.
        logger.LogInformation("Retrieving user for email {Email}", request.Email);
        User? user = await dbContext.Users.FirstOrDefaultAsync(x =>
            x.Email == request.Email && x.PasswordHash == HashingUtility.ComputeSha256Hash(request.Password), token);
        if (user is null)
        {
            logger.LogWarning("User not found or incorrect credentials for email {Email}", request.Email);
            return Result<LoginResponse>.Failure(ResultPatternError.BadRequest("Incorrect login or password"));
        }

        // Update token version and save changes.
        logger.LogInformation("Updating user token version for user {UserId}", user.Id);
        user.TokenVersion = Guid.NewGuid();
        await dbContext.SaveChangesAsync(token);

        // Generate a token for the user.
        logger.LogInformation("Generating token for user {UserId}", user.Id);
        Result<LoginResponse> result = await dbContext.GenerateTokenAsync(user, configuration);
        if (!result.IsSuccess)
        {
            logger.LogError("Error generating token for user {UserId}: {Error}", user.Id, result);
            return Result<LoginResponse>.Failure(ResultPatternError.InternalServerError("Error generating token!"));
        }

        // Update user login history.
        string? userAgent = accessor.GetUserAgent();
        string remoteIpAddress = accessor.GetRemoteIpAddress();
        logger.LogInformation("Updating login information for user {UserId}", user.Id);
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
        logger.LogInformation("Saving login history and token information to database.");
        await dbContext.UserLogins.AddAsync(userLogin, token);
        await dbContext.UserTokens.AddAsync(userToken, token);

        int res = await dbContext.SaveChangesAsync(token);
        if (res != 0)
            logger.LogInformation("Login information saved successfully at {Time}", DateTimeOffset.UtcNow);
        else
            logger.LogError("Failed to save information about login at {Time}", DateTimeOffset.UtcNow);

        logger.LogInformation("Finishing LoginAsync at {Time}", DateTimeOffset.UtcNow);
        return Result<LoginResponse>.Success(result.Value);
    }

    public async Task<BaseResult> LogoutAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        logger.LogInformation("Starting LogoutAsync at {Time}", DateTimeOffset.UtcNow);

        Guid? userId = accessor.GetId();
        if (userId is null)
        {
            logger.LogWarning("Invalid user ID during logout.");
            return BaseResult.Failure(ResultPatternError.BadRequest("Invalid user ID."));
        }

        logger.LogInformation("Retrieving user for logout with ID: {UserId}", userId);
        User? user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, token);
        if (user is null)
        {
            logger.LogWarning("User not found during logout with ID: {UserId}", userId);
            return BaseResult.Failure(ResultPatternError.NotFound("User not found."));
        }

        logger.LogInformation("Updating token version and audit info for logout.");
        user.TokenVersion = Guid.NewGuid();
        user.UpdatedBy = userId;
        user.UpdatedByIp!.Add(accessor.GetRemoteIpAddress());
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.Version++;

        int res = await dbContext.SaveChangesAsync(token);
        logger.LogInformation("Finished LogoutAsync at {Time}", DateTimeOffset.UtcNow);
        return res != 0 ? BaseResult.Success() : BaseResult.Failure(ResultPatternError.InternalServerError("Could not logout!"));
    }

    public async Task<BaseResult> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        logger.LogInformation("Starting ChangePasswordAsync at {Time}", DateTimeOffset.UtcNow);

        Guid? userId = accessor.GetId();
        if (userId is null)
        {
            logger.LogWarning("Invalid user ID in ChangePasswordAsync.");
            return BaseResult.Failure(ResultPatternError.BadRequest("Invalid user ID."));
        }

        logger.LogInformation("Retrieving user for password change with ID: {UserId}", userId);
        User? user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, token);
        if (user is null)
        {
            logger.LogWarning("User not found for ChangePasswordAsync with ID: {UserId}", userId);
            return BaseResult.Failure(ResultPatternError.NotFound("User not found."));
        }

        string hashedOldPassword = HashingUtility.ComputeSha256Hash(request.OldPassword);
        if (user.PasswordHash != hashedOldPassword)
        {
            logger.LogWarning("Old password does not match for user {UserId}", userId);
            return BaseResult.Failure(ResultPatternError.BadRequest("Old password is incorrect."));
        }

        logger.LogInformation("Hashing new password and updating user record.");
        user.PasswordHash = HashingUtility.ComputeSha256Hash(request.NewPassword);
        user.UpdatedBy = userId;
        user.UpdatedByIp!.Add(accessor.GetRemoteIpAddress());
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.Version++;
        user.LastPasswordChangeAt = DateTimeOffset.UtcNow;
        user.TokenVersion = Guid.NewGuid();

        int res = await dbContext.SaveChangesAsync(token);
        if (res == 0)
        {
            logger.LogError("Failed to save password change for user {UserId}", userId);
            return BaseResult.Failure(ResultPatternError.InternalServerError("Could not change password."));
        }

        logger.LogInformation("Password changed successfully for user {UserId} at {Time}", userId, DateTimeOffset.UtcNow);
        logger.LogInformation("Finishing ChangePasswordAsync at {Time}", DateTimeOffset.UtcNow);
        return BaseResult.Success();
    }

    public async Task<BaseResult> SendEmailConfirmationCodeAsync(SendEmailConfirmationCodeRequest request, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        logger.LogInformation("Starting SendEmailConfirmationCodeAsync at {Time}", DateTimeOffset.UtcNow);

        logger.LogInformation("Retrieving user with email {Email}", request.Email);
        User? user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == request.Email, token);
        if (user is null)
        {
            logger.LogWarning("User with email {Email} not found.", request.Email);
            return BaseResult.Failure(ResultPatternError.NotFound("User with this email was not found."));
        }

        if (user.EmailConfirmed)
        {
            logger.LogWarning("Email for user {Email} is already confirmed.", request.Email);
            return BaseResult.Failure(ResultPatternError.BadRequest("Your email address already confirmed"));
        }

        logger.LogInformation("Generating email confirmation code.");
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

        logger.LogInformation("Adding email confirmation code to the database.");
        await dbContext.UserVerificationCodes.AddAsync(userVerificationCode, token);
        int res = await dbContext.SaveChangesAsync(token);
        if (res == 0)
        {
            logger.LogError("Failed to generate email confirmation code for user {Email}", request.Email);
            return BaseResult.Failure(ResultPatternError.InternalServerError("Could not generate email confirmation code."));
        }

        logger.LogInformation("Sending email confirmation code to {Email}", request.Email);
        BaseResult emailResult = await emailService.SendEmailAsync(request.Email,
            "Email Confirmation Code",
            $"Your email confirmation code is: {verificationCode}");
        if (!emailResult.IsSuccess)
        {
            logger.LogError("Failed to send email confirmation code to {Email}", request.Email);
            return BaseResult.Failure(ResultPatternError.InternalServerError("Failed to send email confirmation code."));
        }

        logger.LogInformation("Email confirmation code sent successfully to {Email}", request.Email);
        logger.LogInformation("Finishing SendEmailConfirmationCodeAsync at {Time}", DateTimeOffset.UtcNow);
        return BaseResult.Success();
    }

    public async Task<BaseResult> ConfirmEmailAsync(ConfirmEmailCodeRequest request, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        logger.LogInformation("Starting ConfirmEmailAsync at {Time}", DateTimeOffset.UtcNow);

        logger.LogInformation("Retrieving user with email {Email}", request.Email);
        User? user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == request.Email, token);
        if (user is null)
        {
            logger.LogWarning("User with email {Email} not found.", request.Email);
            return BaseResult.Failure(ResultPatternError.NotFound("User with this email was not found."));
        }

        logger.LogInformation("Retrieving latest email confirmation code for user {UserId}", user.Id);
        UserVerificationCode? verificationCode = await dbContext.UserVerificationCodes
            .Where(x => x.UserId == user.Id && x.Type == VerificationCodeType.EmailConfirmation)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(token);

        if (verificationCode is null ||
            verificationCode.Code != long.Parse(request.Code) ||
            (verificationCode.ExpiryTime - DateTimeOffset.UtcNow).TotalSeconds >= 60)
        {
            logger.LogWarning("Invalid or expired verification code for user {UserId}", user.Id);
            return BaseResult.Failure(ResultPatternError.BadRequest("Invalid or expired reset code."));
        }

        if (verificationCode.Code != long.Parse(request.Code))
        {
            logger.LogWarning("Verification code does not match for user {UserId}", user.Id);
            return BaseResult.Failure(ResultPatternError.BadRequest("Invalid verification code."));
        }

        logger.LogInformation("Marking user email as confirmed for user {UserId}", user.Id);
        user.EmailConfirmed = true;
        user.UpdatedBy = user.Id;
        user.UpdatedByIp!.Add(accessor.GetRemoteIpAddress());
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.Version++;

        int res = await dbContext.SaveChangesAsync(token);
        if (res == 0)
        {
            logger.LogError("Failed to update user record for email confirmation for user {UserId}", user.Id);
            return BaseResult.Failure(ResultPatternError.InternalServerError("Could not confirm email."));
        }

        logger.LogInformation("Sending email confirmation notification to {Email}", user.Email);
        BaseResult emailResult = await emailService.SendEmailAsync(user.Email,
            "Your Email Has Been Confirmed",
            "Congratulations! Your email has been successfully confirmed. You can now use all features of OASIS Bridge.");
        if (!emailResult.IsSuccess)
            logger.LogError("Failed to send email confirmation notification to {Email}", user.Email);

        logger.LogInformation("Finishing ConfirmEmailAsync at {Time}", DateTimeOffset.UtcNow);
        return BaseResult.Success();
    }

    public async Task<BaseResult> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        logger.LogInformation("Starting ForgotPasswordAsync at {Time}", DateTimeOffset.UtcNow);

        logger.LogInformation("Retrieving user with email {EmailAddress}", request.EmailAddress);
        User? user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == request.EmailAddress, token);
        if (user is null)
        {
            logger.LogWarning("User with email {EmailAddress} not found.", request.EmailAddress);
            return BaseResult.Failure(ResultPatternError.NotFound("User with this email was not found."));
        }

        logger.LogInformation("Generating password reset code for user {UserId}", user.Id);
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

        logger.LogInformation("Adding password reset code to the database.");
        await dbContext.UserVerificationCodes.AddAsync(resetCode, token);
        int res = await dbContext.SaveChangesAsync(token);
        if (res == 0)
        {
            logger.LogError("Failed to save password reset code for user {UserId}", user.Id);
            return BaseResult.Failure(ResultPatternError.InternalServerError("Could not generate password reset code."));
        }

        logger.LogInformation("Sending password reset email to {EmailAddress}", request.EmailAddress);
        BaseResult emailResult = await emailService.SendEmailAsync(request.EmailAddress,
            "Password Reset Request",
            $"You have requested to reset your password. Use this code to proceed: {resetCode.Code}. The code is valid for 1 minute.");
        if (!emailResult.IsSuccess)
            logger.LogError("Failed to send password reset email to {EmailAddress}", request.EmailAddress);

        logger.LogInformation("Finishing ForgotPasswordAsync at {Time}", DateTimeOffset.UtcNow);
        return BaseResult.Success();
    }

    public async Task<BaseResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        logger.LogInformation("Starting ResetPasswordAsync at {Time}", DateTimeOffset.UtcNow);

        logger.LogInformation("Retrieving user with email {EmailAddress}", request.EmailAddress);
        User? user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == request.EmailAddress, token);
        if (user is null)
        {
            logger.LogWarning("User with email {EmailAddress} not found.", request.EmailAddress);
            return BaseResult.Failure(ResultPatternError.NotFound("User with this email was not found."));
        }

        logger.LogInformation("Retrieving latest password reset code for user {UserId}", user.Id);
        UserVerificationCode? resetCode = await dbContext.UserVerificationCodes
            .Where(x => x.UserId == user.Id && x.Type == VerificationCodeType.PasswordReset)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(token);

        if (resetCode is null)
        {
            logger.LogWarning("No password reset code found for user {UserId}", user.Id);
            return BaseResult.Failure(ResultPatternError.BadRequest("Invalid reset code."));
        }

        if (resetCode.ExpiryTime < DateTimeOffset.UtcNow || (resetCode.ExpiryTime - DateTimeOffset.UtcNow).TotalSeconds >= 60)
        {
            logger.LogWarning("Password reset code expired for user {UserId}", user.Id);
            return BaseResult.Failure(ResultPatternError.BadRequest("Reset code has expired."));
        }

        logger.LogInformation("Updating user password for user {UserId}", user.Id);
        user.PasswordHash = HashingUtility.ComputeSha256Hash(request.NewPassword);
        user.UpdatedBy = user.Id;
        user.UpdatedByIp!.Add(accessor.GetRemoteIpAddress());
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.Version++;
        user.TokenVersion = Guid.NewGuid();

        int res = await dbContext.SaveChangesAsync(token);
        if (res == 0)
        {
            logger.LogError("Failed to update password for user {UserId}", user.Id);
            return BaseResult.Failure(ResultPatternError.InternalServerError("Could not reset password."));
        }

        logger.LogInformation("Sending password reset confirmation email to {Email}", request.EmailAddress);
        BaseResult emailResult = await emailService.SendEmailAsync(request.EmailAddress,
            "Password Successfully Reset",
            "Your password has been successfully reset. If you did not request this change, please contact support immediately.");
        if (!emailResult.IsSuccess)
            logger.LogError("Failed to send password reset confirmation email to {Email}", request.EmailAddress);

        logger.LogInformation("Finishing ResetPasswordAsync at {Time}", DateTimeOffset.UtcNow);
        return BaseResult.Success();
    }

    public async Task<BaseResult> RestoreAccountAsync(RestoreAccountRequest request, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        logger.LogInformation("Starting RestoreAccountAsync at {Time}", DateTimeOffset.UtcNow);

        logger.LogInformation("Retrieving user with email {Email} for account restoration.", request.Email);
        User? user = await dbContext.Users.IgnoreQueryFilters().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == request.Email, token);
        if (user is null)
        {
            logger.LogWarning("User with email {Email} not found.", request.Email);
            return BaseResult.Failure(ResultPatternError.NotFound("User with this email was not found."));
        }

        if (!user.IsDeleted)
        {
            logger.LogWarning("Account for user {UserId} is already active.", user.Id);
            return BaseResult.Failure(ResultPatternError.BadRequest("This account is already active."));
        }

        logger.LogInformation("Generating restore verification code for user {UserId}.", user.Id);
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

        logger.LogInformation("Adding restore code to database.");
        await dbContext.UserVerificationCodes.AddAsync(restoreCode, token);
        int res = await dbContext.SaveChangesAsync(token);
        if (res == 0)
        {
            logger.LogError("Failed to save restore code for user {UserId}", user.Id);
            return BaseResult.Failure(ResultPatternError.InternalServerError("Could not restore code"));
        }

        logger.LogInformation("Sending restore account email to {Email}.", request.Email);
        BaseResult emailResult = await emailService.SendEmailAsync(request.Email,
            "Restore Your OASIS Bridge Account",
            $"Use this verification code to restore your account: {restoreCode.Code}");
        if (!emailResult.IsSuccess)
            logger.LogError("Failed to send restore account email to {Email}.", request.Email);

        logger.LogInformation("Finishing RestoreAccountAsync at {Time}", DateTimeOffset.UtcNow);
        return BaseResult.Success("Verification code has been sent to your email.");
    }

    public async Task<BaseResult> ConfirmRestoreAccountAsync(ConfirmRestoreAccountRequest request, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        logger.LogInformation("Starting ConfirmRestoreAccountAsync at {Time}", DateTimeOffset.UtcNow);

        logger.LogInformation("Retrieving user with email {Email} for restore confirmation.", request.Email);
        User? user = await dbContext.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Email == request.Email, token);
        if (user is null)
        {
            logger.LogWarning("User with email {Email} not found.", request.Email);
            return BaseResult.Failure(ResultPatternError.NotFound("User with this email was not found."));
        }

        if (!user.IsDeleted)
        {
            logger.LogWarning("Account for user {UserId} is already active.", user.Id);
            return BaseResult.Failure(ResultPatternError.BadRequest("This account is already active."));
        }

        logger.LogInformation("Retrieving restore verification code for user {UserId}.", user.Id);
        UserVerificationCode? restoreCode = await dbContext.UserVerificationCodes
            .Where(x => x.UserId == user.Id && x.Code == long.Parse(request.Code) && x.Type == VerificationCodeType.AccountRestore)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(token);

        if (restoreCode is null)
        {
            logger.LogWarning("Invalid restore code for user {UserId}.", user.Id);
            return BaseResult.Failure(ResultPatternError.BadRequest("Invalid restore code."));
        }

        double difference = (DateTimeOffset.UtcNow - user.DeletedAt!.Value).TotalDays;
        if (difference > 30)
        {
            logger.LogWarning("Restore code expired for user {UserId}.", user.Id);
            return BaseResult.Failure(ResultPatternError.BadRequest("The restore code has expired. Please contact technical support to restore your account."));
        }

        if (restoreCode.ExpiryTime < DateTimeOffset.UtcNow || (restoreCode.ExpiryTime - DateTimeOffset.UtcNow).TotalSeconds >= 60)
        {
            logger.LogWarning("Restore code has expired for user {UserId}.", user.Id);
            return BaseResult.Failure(ResultPatternError.BadRequest("Reset code has expired."));
        }

        logger.LogInformation("Restoring account for user {UserId}.", user.Id);
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
        {
            logger.LogError("Failed to restore account for user {UserId}.", user.Id);
            return BaseResult.Failure(ResultPatternError.InternalServerError("Could not restore account."));
        }

        logger.LogInformation("Sending account restored email to {Email}.", request.Email);
        BaseResult emailResult = await emailService.SendEmailAsync(request.Email,
            "Account Restored",
            "Your account has been successfully restored. If you did not request this, please contact support immediately.");
        if (!emailResult.IsSuccess)
            logger.LogError("Failed to send account restored email to {Email}.", request.Email);

        logger.LogInformation("Finishing ConfirmRestoreAccountAsync at {Time}", DateTimeOffset.UtcNow);
        return BaseResult.Success("Account successfully restored.");
    }

    public async Task<BaseResult> DeleteAccountAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        logger.LogInformation("Starting DeleteAccountAsync at {Time}", DateTimeOffset.UtcNow);

        Guid userId = accessor.GetId() ?? HttpAccessor.SystemId;
        string remoteIpAddress = accessor.GetRemoteIpAddress();
        logger.LogInformation("Retrieving user with ID {UserId} for deletion.", userId);

        User? user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, token);
        if (user is null)
        {
            logger.LogWarning("User with ID {UserId} not found.", userId);
            return BaseResult.Failure(ResultPatternError.NotFound("User not found."));
        }

        if (user.IsDeleted)
        {
            logger.LogWarning("User with ID {UserId} is already deleted.", userId);
            return BaseResult.Failure(ResultPatternError.BadRequest("Account is already deleted."));
        }

        logger.LogInformation("Marking user {UserId} as deleted.", userId);
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
        {
            logger.LogError("Failed to mark user {UserId} as deleted.", userId);
            return BaseResult.Failure(ResultPatternError.InternalServerError("Could not delete account."));
        }

        logger.LogInformation("Sending account deletion email to {Email}.", user.Email);
        BaseResult emailResult = await emailService.SendEmailAsync(user.Email,
            "Account Deleted",
            "Your account has been successfully deleted. If you did not request this, please contact support immediately. " +
            "You can restore your account within the next 30 days by using the restore feature in your account settings.");
        if (!emailResult.IsSuccess)
            logger.LogError("Failed to send account deletion email to {Email}.", user.Email);

        logger.LogInformation("Finishing DeleteAccountAsync at {Time}", DateTimeOffset.UtcNow);
        return BaseResult.Success("Account successfully deleted.");
    }
}