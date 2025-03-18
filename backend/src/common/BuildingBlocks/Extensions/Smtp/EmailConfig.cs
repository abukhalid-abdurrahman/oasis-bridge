namespace BuildingBlocks.Extensions.Smtp;

public sealed class EmailConfig
{
    public required string SmtpServer { get; init; }
    public required int SmtpPort { get; init; }
    public required string SenderEmailAddress { get; init; }
    public required string SenderName { get; init; } = "OASIS";
    public required string AppPassword { get; init; }
    public bool EnableSsl { get; init; } = true;
    public int Timeout { get; init; } = 10000;
    public int MaxRetryAttempts { get; init; } = 3;
    public int RetryDelay { get; init; } = 3000;
}