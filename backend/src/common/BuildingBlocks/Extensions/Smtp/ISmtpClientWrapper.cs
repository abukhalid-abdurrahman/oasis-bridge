namespace BuildingBlocks.Extensions.Smtp;

public interface ISmtpClientWrapper
{
    Task<BaseResult> ConnectAsync();
    Task<BaseResult> SendMessageAsync(MimeMessage emailMessage);
    Task<BaseResult> DisconnectAsync();
}