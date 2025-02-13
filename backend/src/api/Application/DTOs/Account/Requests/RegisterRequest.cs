namespace Application.DTOs.Account.Requests;

public sealed record RegisterRequest
{
    [Required, EmailAddress]
    public string EmailAddress { get; init; } = string.Empty;

    [Required, RegularExpression(@"^\+?[1-9]\d{1,14}$",
         ErrorMessage = "Invalid phone number format.")]
    public string PhoneNumber { get; init; } = string.Empty;

    [Required, MinLength(4), MaxLength(128)]
    public string UserName { get; init; } = string.Empty;

    [Required, MinLength(8), MaxLength(128)]
    public string Password { get; init; } = string.Empty;

    [Required, Compare(nameof(Password),
         ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; init; } = string.Empty;
}