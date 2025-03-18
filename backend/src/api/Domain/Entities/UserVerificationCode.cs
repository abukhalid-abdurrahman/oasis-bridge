namespace Domain.Entities;

public sealed class UserVerificationCode : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public long Code { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset ExpiryTime { get; set; }

    public VerificationCodeType Type { get; set; }
    public bool IsUsed { get; set; }
}