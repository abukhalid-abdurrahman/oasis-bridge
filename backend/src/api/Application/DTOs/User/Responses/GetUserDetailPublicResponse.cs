namespace Application.DTOs.User.Responses;

public sealed record GetUserDetailPublicResponse(
    Guid Id,
    string? FirstName,
    string? LastName,
    string Email,
    string PhoneNumber,
    string UserName,
    DateTimeOffset? Dob
);