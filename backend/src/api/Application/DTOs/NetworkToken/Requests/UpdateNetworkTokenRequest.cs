namespace Application.DTOs.NetworkToken.Requests;

public record UpdateNetworkTokenRequest(
    string Symbol,
    string? Description,
    Guid NetworkId
);