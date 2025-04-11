namespace Domain.Entities;

public sealed class WalletLinkedAccount : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public Guid NetworkId { get; set; }
    public Network Network { get; set; } = default!;

    public string PublicKey { get; set; } = string.Empty;
    
    public DateTimeOffset LinkedAt { get; set; } = DateTimeOffset.UtcNow;
}