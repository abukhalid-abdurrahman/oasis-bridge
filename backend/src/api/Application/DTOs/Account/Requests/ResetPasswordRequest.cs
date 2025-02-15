namespace Application.DTOs.Account.Requests;

public sealed record ResetPasswordRequest
{
    [Required, EmailAddress] public string EmailAddress { get; init; } = string.Empty;

    [Required, RegularExpression(@"^\d{6}$",
         ErrorMessage = "The reset code must be a 6-digit number.")]
    public string ResetCode { get; init; } = string.Empty;

    [Required, MinLength(8), MaxLength(128)]
    public string NewPassword { get; init; } = string.Empty;

    [Required, Compare(nameof(NewPassword),
         ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; init; } = string.Empty;
}