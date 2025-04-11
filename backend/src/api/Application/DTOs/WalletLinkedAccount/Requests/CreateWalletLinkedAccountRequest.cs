namespace Application.DTOs.WalletLinkedAccount.Requests;

public sealed record CreateWalletLinkedAccountRequest(
    string WalletAddress,
    string Network);