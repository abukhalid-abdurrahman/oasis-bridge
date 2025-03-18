namespace Application.DTOs.NetworkToken.Responses;

public record GetNetworkTokenResponse(
    Guid Id,
    string Symbol,
    string? Description,
    Guid NetworkId
);