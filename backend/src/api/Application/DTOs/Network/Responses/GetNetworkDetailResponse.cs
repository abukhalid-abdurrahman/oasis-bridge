namespace Application.DTOs.Network.Responses;

public record GetNetworkDetailResponse(
    Guid Id,
    string Name,
    string? Description,
    List<string> Tokens);