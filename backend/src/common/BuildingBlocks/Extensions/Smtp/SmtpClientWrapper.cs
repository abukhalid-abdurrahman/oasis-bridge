namespace BuildingBlocks.Extensions.Smtp;

public sealed class SmtpClientWrapper(EmailConfig emailConfig, SmtpClient client) : ISmtpClientWrapper
{
    public async Task<BaseResult> ConnectAsync()
    {
        for (int attempt = 1; attempt <= emailConfig.MaxRetryAttempts; attempt++)
        {
            try
            {
                client.Timeout = emailConfig.Timeout;
                await client.ConnectAsync(emailConfig.SmtpServer, emailConfig.SmtpPort, emailConfig.EnableSsl);
                await client.AuthenticateAsync(emailConfig.SenderEmailAddress, emailConfig.AppPassword);
                return BaseResult.Success();
            }
            catch (Exception ex)
            {
                if (attempt == emailConfig.MaxRetryAttempts)
                {
                    return BaseResult.Failure(
                        ResultPatternError.InternalServerError($"SMTP Connection Failed: {ex.Message}"));
                }

                await Task.Delay(emailConfig.RetryDelay);
            }
        }

        return BaseResult.Failure(
            ResultPatternError.InternalServerError("SMTP Connection failed after multiple attempts."));
    }

    public async Task<BaseResult> SendMessageAsync(MimeMessage emailMessage)
    {
        try
        {
            await client.SendAsync(emailMessage);
            return BaseResult.Success();
        }
        catch (Exception ex)
        {
            return BaseResult.Failure(ResultPatternError.InternalServerError($"Email Sending Failed: {ex.Message}"));
        }
    }

    public async Task<BaseResult> DisconnectAsync()
    {
        try
        {
            await client.DisconnectAsync(true);
            return BaseResult.Success();
        }
        catch (Exception ex)
        {
            return BaseResult.Failure(ResultPatternError.InternalServerError($"SMTP Disconnect Failed: {ex.Message}"));
        }
    }
}