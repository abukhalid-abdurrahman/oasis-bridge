namespace Domain.Entities;

public sealed class NetworkToken : BaseEntity
{
    public string Symbol { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Guid NetworkId { get; set; }
    public Network Network { get; set; } = default!;

    public ICollection<AccountBalance> AccountBalances { get; } = [];
}