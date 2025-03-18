namespace Domain.Entities;

public sealed class VirtualAccount : BaseEntity
{
    public string PrivateKey { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string SeedPhrase { get; set; } = string.Empty;
    public NetworkType NetworkType { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public Guid NetworkId { get; set; }
    public Network Network { get; set; } = default!;
    
    public ICollection<AccountBalance> Balances { get; set; } = [];

}