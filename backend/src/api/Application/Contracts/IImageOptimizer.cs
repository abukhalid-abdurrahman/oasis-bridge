namespace Application.Contracts;

/// <summary>
/// Defines a contract for optimizing image data, such as resizing, re-encoding, or compressing.
/// Intended to reduce image size or convert to more efficient formats for performance and delivery.
/// </summary>
public interface IImageOptimizer
{
    /// <summary>
    /// Optimizes the provided image byte array.
    /// This may include resizing, format conversion (e.g. WebP), or compression.
    /// </summary>
    /// <param name="imageData">The raw image data to optimize.</param>
    /// <returns>
    /// A byte array containing the optimized image data.
    /// </returns>
    Task<byte[]> OptimizeImageAsync(byte[] imageData);
}