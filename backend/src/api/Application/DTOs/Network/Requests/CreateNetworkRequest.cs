namespace Application.DTOs.Network.Requests;

public record CreateNetworkRequest(
    string Name,
    string? Description,
    NetworkType NetworkType
);