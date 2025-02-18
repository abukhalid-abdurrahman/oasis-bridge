namespace Application.DTOs.VirtualAccount.Responses;

public record GetVirtualAccountDetailResponse(
    string Address,
    string Network,
    List<(string TokenName, decimal Balance)> Tokens
);