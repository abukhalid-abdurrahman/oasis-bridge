namespace Domain.Entities;

public sealed class Order : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public Guid ExchangeRateId { get; set; }
    public ExchangeRate ExchangeRate { get; set; } = default!;

    public string FromNetwork { get; set; } = string.Empty;
    public string FromToken { get; set; } = string.Empty;

    public string ToNetwork { get; set; } = string.Empty;
    public string ToToken { get; set; } = string.Empty;

    public string DestinationAddress { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public string? TransactionHash { get; set; } = string.Empty;

    public OrderStatus OrderStatus { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}