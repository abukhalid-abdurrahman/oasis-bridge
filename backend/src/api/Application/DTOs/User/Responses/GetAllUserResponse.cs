namespace Application.DTOs.User.Responses;

public sealed record GetAllUserResponse(
    string? FirstName,
    string? LastName,
    string Email,
    string PhoneNumber,
    string UserName,
    DateTimeOffset? Dob
);