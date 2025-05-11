namespace Application.DTOs.RwaTokenPriceHistory.Responses;

public readonly record struct GetRwaTokenPriceHistoryResponse(
    Guid Id,
    Guid RwaTokenId,
    decimal OldPrice,
    decimal NewPrice,
    DateTime ChangedAt,
    Guid VirtualAccountOwnerId,
    string PublicKey,
    Guid OwnerUserId,
    string Email
);