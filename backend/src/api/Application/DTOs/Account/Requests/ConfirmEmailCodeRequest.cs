namespace Application.DTOs.Account.Requests;

public sealed record ConfirmEmailCodeRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6), MaxLength(6)] string Code
);