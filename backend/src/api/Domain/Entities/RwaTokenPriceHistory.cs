namespace Domain.Entities;

public sealed class RwaTokenPriceHistory : BaseEntity
{
    public Guid RwaTokenId { get; set; }
    public RwaToken RwaToken { get; set; } = default!;

    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public DateTime ChangedAt { get; set; }

    public Guid CurrentOwnerId { get; set; }
    public VirtualAccount CurrentOwner { get; set; } = default!;
}