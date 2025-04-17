namespace Common.Validations;

public static class IpfsValidator
{
    private static readonly Regex CidV0Regex = new("^Qm[1-9A-HJ-NP-Za-km-z]{44}$", RegexOptions.Compiled);

    public static bool IsValidCidV0(string cid)
    {
        if (string.IsNullOrWhiteSpace(cid))
            return false;

        return CidV0Regex.IsMatch(cid);
    }
}