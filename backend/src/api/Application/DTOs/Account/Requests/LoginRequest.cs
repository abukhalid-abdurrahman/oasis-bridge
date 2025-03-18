namespace Application.DTOs.Account.Requests;

public sealed record LoginRequest(
    [Required, MinLength(4), MaxLength(128),EmailAddress]
    string Email,
    [Required, MinLength(8), MaxLength(128)]
    string Password);