namespace Domain.Entities;

public sealed class UserLogin : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public ProviderType Provider { get; set; } = ProviderType.Local;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool Successful { get; set; }
}