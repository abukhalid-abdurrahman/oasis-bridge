namespace Application.DTOs.Account.Responses;

public sealed record LoginResponse(
    string Token, 
    DateTimeOffset StartTime, 
    DateTimeOffset ExpiresAt
);
