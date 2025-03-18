namespace Domain.Entities;

public sealed class AccountBalance : BaseEntity
{
    public decimal Balance { get; set; }
    
    public Guid VirtualAccountId { get; set; }
    public VirtualAccount VirtualAccount { get; set; } = default!;

    public Guid NetworkTokenId { get; set; }
    public NetworkToken NetworkToken { get; set; } = default!;
}