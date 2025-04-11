namespace Application.DTOs.WalletLinkedAccount.Responses;

public sealed record GetWalletLinkedAccountDetailResponse(
    Guid UserId,
    string WalletAddress,
    string Network,
    DateTimeOffset LinkedAt);