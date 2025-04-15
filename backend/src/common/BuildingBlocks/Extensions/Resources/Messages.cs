namespace BuildingBlocks.Extensions.Resources;

public static class Messages
{
    private static readonly ResourceManager _resources = new(typeof(Messages).FullName!, typeof(Messages).Assembly);

    public static string WalletLinkedAccountFailed => _resources.Get().AsString();
    public static string RegisterUserFailed => _resources.Get().AsString();
    public static string EmailFailed => _resources.Get().AsString();
    public static string ConfirmRestoreAccountFailed => _resources.Get().AsString();
    public static string LoginUserFailed => _resources.Get().AsString();
    public static string LogoutUserFailed => _resources.Get().AsString();
    public static string GenerateTokenFailed => _resources.Get().AsString();
    public static string RestoreAccountFailed => _resources.Get().AsString();
    public static string DeleteAccountFailed => _resources.Get().AsString();
    public static string ChangePasswordUserFailed => _resources.Get().AsString();
    public static string SendEmailConfirmationCodeFailed => _resources.Get().AsString();
    public static string ResetPasswordFailed => _resources.Get().AsString();
    public static string ConfirmEmailFailed => _resources.Get().AsString();
    public static string ForgotPasswordFailed => _resources.Get().AsString();
    public static string ConfirmEmailInvalidOrExpiredTimeCode => _resources.Get().AsString();
    public static string UserNotFound => _resources.Get().AsString();
    public static string NetworkNotFound => _resources.Get().AsString();
    public static string ExchangeRateNotFound => _resources.Get().AsString();
    public static string RoleNotFound => _resources.Get().AsString();
    public static string UserAlreadyExist => _resources.Get().AsString();
    public static string WalletLinkedAccountAlreadyExist => _resources.Get().AsString();
    public static string LoginUserIncorrect => _resources.Get().AsString();
    public static string ChangePasswordIncorrect => _resources.Get().AsString();
    public static string ConfirmEmailSubjectMessage => _resources.Get().AsString();
    public static string ConfirmEmailBodyMessage => _resources.Get().AsString();
   public static string ConfirmRestoreAccountSubjectMessage => _resources.Get().AsString();
    public static string ConfirmRestoreAccountBodyMessage => _resources.Get().AsString();
    public static string SendEmailConfirmationSubjectMessage => _resources.Get().AsString();
    public static string ForgotPasswordSubjectMessage => _resources.Get().AsString();
    public static string ForgotPasswordBody1Message => _resources.Get().AsString();
    public static string ForgotPasswordBody2Message => _resources.Get().AsString();
    public static string ResetPasswordInvalidOrExpiredCode => _resources.Get().AsString();
    public static string ResetPasswordSubjectMessage => _resources.Get().AsString();
    public static string RestoreAccountSubjectMessage => _resources.Get().AsString();
    public static string RestoreAccountBodyMessage => _resources.Get().AsString();
    public static string ResetPasswordBodyMessage => _resources.Get().AsString();  
    public static string DeleteAccountSubjectMessage => _resources.Get().AsString();
    public static string DeleteAccountBodyMessage => _resources.Get().AsString();
    public static string AccountAlreadyActive => _resources.Get().AsString();
    public static string ConfirmRestoreAccountInvalidOrExpiredCode => _resources.Get().AsString();
    
}