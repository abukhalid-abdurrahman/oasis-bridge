namespace Domain.Entities;

public sealed class UserClaim : BaseEntity
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
}