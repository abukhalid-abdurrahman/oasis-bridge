namespace Application.DTOs.Account.Requests;

public sealed record ForgotPasswordRequest(
    [Required, EmailAddress] string EmailAddress);