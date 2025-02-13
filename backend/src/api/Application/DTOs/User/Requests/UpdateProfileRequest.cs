namespace Application.DTOs.User.Requests;

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string UserName,
    DateTimeOffset? Dob);