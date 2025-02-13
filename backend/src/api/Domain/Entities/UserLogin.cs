namespace Domain.Entities;

public sealed class UserLogin : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public LoginProviderType LoginProvider { get; set; } = LoginProviderType.Local;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool Successful { get; set; }
}