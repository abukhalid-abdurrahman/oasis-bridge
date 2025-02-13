namespace Application.DTOs.User.Responses;

public sealed record GetUserDetailPrivateResponse(
    string? FirstName,
    string? LastName,
    string Email,
    string PhoneNumber,
    string UserName,
    DateTimeOffset? Dob,
    DateTimeOffset? LastLoginAt,
    long TotalLogins
);