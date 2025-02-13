namespace Application.DTOs.Account.Responses;

public class ConfirmRestoreAccountResponse(
    string Token,
    DateTimeOffset StartTime,
    DateTimeOffset ExpiresAt);