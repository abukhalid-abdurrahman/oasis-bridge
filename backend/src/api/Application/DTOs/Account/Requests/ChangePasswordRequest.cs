namespace Application.DTOs.Account.Requests
{
    /// <summary>
    /// Represents a request to change the user's password.
    /// Contains the user's current password, the new password, and a confirmation of the new password.
    /// </summary>
    /// <param name="OldPassword">
    /// The user's current password, which is required for authentication before allowing the change.
    /// </param>
    /// <param name="NewPassword">
    /// The new password that the user wants to set.
    /// </param>
    /// <param name="ConfirmPassword">
    /// A confirmation of the new password to ensure both passwords match.
    /// </param>
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
}