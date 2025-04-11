namespace BuildingBlocks.Extensions.Smtp;

public sealed class EmailConfig
{
    public required string SmtpServer { get; init; }
    public required int SmtpPort { get; init; }
    public required string SenderEmailAddress { get; init; }
    public required string SenderName { get; init; } 
    public required string AppPassword { get; init; }
    public bool EnableSsl { get; init; } = true;
    public int Timeout { get; init; } 
    public int MaxRetryAttempts { get; init; } 
    public int RetryDelay { get; init; }
}