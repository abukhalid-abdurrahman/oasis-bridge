namespace Application.DTOs.Account.Requests;

public sealed record ChangePasswordRequest
{
    [Required, MinLength(8), MaxLength(128)]
    public string OldPassword { get; init; } = string.Empty;

    [Required, MinLength(8), MaxLength(128)]
    public string NewPassword { get; init; } = string.Empty;

    [Required, Compare(nameof(NewPassword),
         ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; init; } = string.Empty;
}