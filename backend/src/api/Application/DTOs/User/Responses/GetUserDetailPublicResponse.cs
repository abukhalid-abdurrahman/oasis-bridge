namespace Application.DTOs.User.Responses;

public sealed record GetUserDetailPublicResponse(
    string? FirstName,
    string? LastName,
    string Email,
    string PhoneNumber,
    string UserName,
    DateTimeOffset? Dob
);