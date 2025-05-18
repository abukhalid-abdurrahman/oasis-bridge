namespace Application.Extensions.Mappers;

public static class RwaTokenPriceHistoryMapper
{
    public static GetRwaTokenPriceHistoryResponse ToRead(this RwaTokenPriceHistory priceHistory)
        => new(
            priceHistory.Id,
            priceHistory.RwaTokenId,
            priceHistory.OldPrice,
            priceHistory.NewPrice,
            priceHistory.ChangedAt,
            priceHistory.RwaToken.VirtualAccountId
            ?? priceHistory.RwaToken.WalletLinkedAccountId,
            priceHistory.RwaToken.VirtualAccount?.PublicKey
            ?? priceHistory.RwaToken.WalletLinkedAccount?.PublicKey,
            priceHistory.RwaToken.VirtualAccount?.UserId ??
            priceHistory.RwaToken.WalletLinkedAccount?.UserId);
}