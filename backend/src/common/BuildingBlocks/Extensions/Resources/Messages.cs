namespace BuildingBlocks.Extensions.Resources;

public static class Messages
{
    private static readonly ResourceManager _resources = new(typeof(Messages).FullName!, typeof(Messages).Assembly!);

    public static string WalletLinkedAccountFailed => _resources.Get().AsString();
    public static string UserNotFound => _resources.Get().AsString();
    public static string NetworkNotFound => _resources.Get().AsString();
    public static string WalletLinkedAccountAlreadyExist => _resources.Get().AsString();
    public static string ExchangeRateNotFound => _resources.Get().AsString();
}