namespace Domain.Entities;

public sealed class Network : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public NetworkType NetworkType { get; set; }

    public ICollection<VirtualAccount> VirtualAccounts { get; set; } = [];
    public ICollection<NetworkToken> NetworkTokens { get; set; } = [];
}