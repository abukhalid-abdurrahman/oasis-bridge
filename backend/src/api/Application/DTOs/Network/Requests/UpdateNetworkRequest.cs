namespace Application.DTOs.Network.Requests;

public record UpdateNetworkRequest(
    string Name,
    string? Description,
    NetworkType NetworkType
);