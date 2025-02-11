namespace RadixBridge.Helpers;

/// <summary>
/// Provides helper methods related to the Radix network, including network constants and utility functions.
/// </summary>
public static class RadixBridgeHelper
{
    // Constants representing the network identifiers for StokeNet and MainNet.
    public const string StokeNet = "stokenet"; // The network identifier for StokeNet.
    public const string MainNet = "mainnet"; // The network identifier for MainNet.

    // Constants representing the XRD resource addresses for StokeNet and MainNet.
    public const string
        MainNetXrdAddress =
            "resource_rdx1tknxxxxxxxxxradxrdxxxxxxxxx009923554798xxxxxxxxxradxrd"; // MainNet XRD address.

    public const string
        StokeNetXrdAddress =
            "resource_tdx_2_1tknxxxxxxxxxradxrdxxxxxxxxx009923554798xxxxxxxxxtfd2jc"; // StokeNet XRD address.

    /// <summary>
    /// Generates a private key from the provided mnemonic (seed phrase).
    /// This method uses the SHA256 hash of the mnemonic to derive the private key.
    /// </summary>
    /// <param name="mnemonic">The mnemonic (seed phrase) used to derive the private key.</param>
    /// <returns>The generated private key associated with the mnemonic.</returns>
    public static PrivateKey GetPrivateKey(Mnemonic mnemonic)
    {
        // Create a SHA256 hash of the mnemonic to derive a seed.
        using SHA256 sha256 = SHA256.Create();
        byte[] seed32Bytes = sha256.ComputeHash(mnemonic.DeriveSeed());

        // Return a new PrivateKey instance derived from the seed using the ED25519 curve.
        return new(seed32Bytes, Curve.ED25519);
    }

    /// <summary>
    /// Generates a random nonce, typically used for transaction uniqueness.
    /// Nonces are used to ensure that transactions are unique and cannot be replayed.
    /// </summary>
    /// <returns>A random nonce value as a uint.</returns>
    public static uint RandomNonce()
    {
        // Generate a random integer and return it as a uint to ensure it's non-negative.
        return (uint)RandomNumberGenerator.GetInt32(int.MaxValue);
    }
}