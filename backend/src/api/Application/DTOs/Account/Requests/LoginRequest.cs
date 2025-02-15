namespace Application.DTOs.Account.Requests;

public sealed record LoginRequest(
    [Required, MinLength(4), MaxLength(128)]
    string Login,
    [Required, MinLength(8), MaxLength(128)]
    string Password);