namespace Application.DTOs.Network.Responses;

public record GetNetworkResponse(
    Guid Id,
    string Name,
    string? Description,
    string NetworkType
);