namespace Application.Extensions.Algorithms;

public static class HashingUtility
{
    public static string ComputeSha256Hash(string input)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes).ToLower();
    }
}