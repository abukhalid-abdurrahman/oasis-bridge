namespace Application.DTOs.NftPurchase.Responses;

public readonly record struct TransactionResponse(
    string Status,
    string Message,
    int Code,
    TransactionData Data
);

public readonly record struct TransactionData(
    string TransactionId);