namespace Application.DTOs.VirtualAccount.Responses;

public record GetVirtualAccountDetailResponse(
    string Address,
    string Network,
    string Token,
    decimal Balance
);