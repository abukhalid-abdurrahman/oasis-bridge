namespace Application.DTOs.NftPurchase.Requests;

public readonly record struct CreateTransactionRequest(
    string BuyerPubkey,
    string SellerPubkey,
    string SellerSecretKey,
    string NftMint,
    decimal Price,
    string? TokenMint);