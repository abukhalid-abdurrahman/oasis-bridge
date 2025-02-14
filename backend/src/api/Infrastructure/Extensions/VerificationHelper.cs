namespace Infrastructure.Extensions;

public static class VerificationHelper
{
    private static readonly Random Random = new();

    public static long GenerateVerificationCode()
        => Random.NextInt64(100000, 999999);
}