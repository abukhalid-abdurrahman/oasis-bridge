namespace Domain.Entities;

public sealed class ExchangeRate : BaseEntity
{
    public Guid FromTokenId { get; set; }

    public NetworkToken FromToken { get; set; } = default!;

    public Guid ToTokenId { get; set; }

    public NetworkToken ToToken { get; set; } = default!;

    public decimal Rate { get; set; }

    public string SourceUrl { get; set; } = default!;

    public ICollection<Order> Orders { get; } = [];
}