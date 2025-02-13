namespace Application.DTOs.Account.Requests;

public sealed record SendEmailConfirmationCodeRequest(
    [Required, EmailAddress] string Email
);