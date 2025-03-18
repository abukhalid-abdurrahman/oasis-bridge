namespace Application.DTOs.NetworkToken.Responses;

public record GetNetworkTokenDetailResponse(
    Guid Id,
    string Symbol,
    string? Description,
    Guid NetworkId
);