namespace Domain.Entities;

public sealed class User : BaseEntity
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    public DateTimeOffset? Dob { get; set; }

    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }

    public string PasswordHash { get; set; } = string.Empty;
    public DateTimeOffset? LastPasswordChangeAt { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public long TotalLogins { get; set; }

    public string? TwoFactorSecret { get; set; }
    public bool TwoFactorEnabled { get; set; }

    public Guid TokenVersion { get; set; } = Guid.NewGuid();

    public HashSet<UserRole> UserRoles { get; } = [];
    public ICollection<UserClaim> UserClaims { get; } = [];
    public ICollection<UserLogin> UserLogins { get; } = [];
    public ICollection<UserToken> UserTokens { get; } = [];
    public ICollection<UserVerificationCode> UserVerificationCodes { get; } = [];
    public ICollection<VirtualAccount> VirtualAccounts { get; } = [];
    public ICollection<Order> Orders { get; } = [];
    public ICollection<WalletLinkedAccount> WalletLinkedAccounts { get; } = [];
}