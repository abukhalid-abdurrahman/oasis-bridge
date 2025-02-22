namespace Domain.Enums;

public enum OrderStatus
{
    Pending,
    SufficientFunds,
    InsufficientFunds,
    Expired,
    Completed,
    Canceled,
    NotFound
}