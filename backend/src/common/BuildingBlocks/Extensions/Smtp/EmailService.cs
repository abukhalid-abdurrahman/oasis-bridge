namespace BuildingBlocks.Extensions.Smtp;

public sealed class EmailService(ISmtpClientWrapper smtpClientWrapper, EmailConfig emailConfig) : IEmailService
{
    public async Task<BaseResult> SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            MimeMessage emailMessage = CreateEmailMessage(toEmail, subject, body);

            BaseResult connectResult = await smtpClientWrapper.ConnectAsync();
            if (!connectResult.IsSuccess) return connectResult;

            BaseResult sendResult = await smtpClientWrapper.SendMessageAsync(emailMessage);
            if (!sendResult.IsSuccess) return sendResult;

            BaseResult disconnectResult = await smtpClientWrapper.DisconnectAsync();
            return disconnectResult;
        }
        catch (Exception ex)
        {
            return Result<BaseResult>.Failure(
                ResultPatternError.InternalServerError($"Unexpected error: {ex.Message}"));
        }
    }

    private MimeMessage CreateEmailMessage(string toEmail, string subject, string body)
        => new()
        {
            From = { new MailboxAddress(emailConfig.SenderName, emailConfig.SenderEmailAddress) },
            To = { new MailboxAddress(toEmail, toEmail) },
            Subject = subject,
            Body = new TextPart(TextFormat.Plain) { Text = body }
        };
}