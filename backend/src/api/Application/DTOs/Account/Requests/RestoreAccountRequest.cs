namespace Application.DTOs.Account.Requests;

public sealed record RestoreAccountRequest(
    [Required, EmailAddress] string Email
);