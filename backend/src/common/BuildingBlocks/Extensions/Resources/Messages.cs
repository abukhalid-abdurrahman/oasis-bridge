using DotNext.Resources;

namespace BuildingBlocks.Extensions.Resources;

public static class Messages
{
    private static readonly ResourceManager _resources = new(typeof(Messages).FullName!, typeof(Messages).Assembly!);

    public static string WalletLinkingFailed => _resources.Get().AsString();
}