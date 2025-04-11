namespace Domain.Entities;

public sealed class Network : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public NetworkType NetworkType { get; set; }

    public ICollection<VirtualAccount> VirtualAccounts { get;} = [];
    public ICollection<NetworkToken> NetworkTokens { get;} = [];
    public ICollection<WalletLinkedAccount> WalletLinkedAccounts { get; } = [];
}