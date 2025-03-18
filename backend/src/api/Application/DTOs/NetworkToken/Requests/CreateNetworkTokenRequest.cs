namespace Application.DTOs.NetworkToken.Requests;

public record CreateNetworkTokenRequest(
    string Symbol,
    string? Description,
    Guid NetworkId
);