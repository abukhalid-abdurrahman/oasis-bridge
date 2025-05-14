namespace Application.DTOs.NftPurchase.Requests;

public readonly record struct CreateTransactionRequest(
    string BuyerPublicKey,
    string SellerPubicKey,
    string SellerPrivateKey,
    string NftMint,
    double Price,
    string? TokenMint);