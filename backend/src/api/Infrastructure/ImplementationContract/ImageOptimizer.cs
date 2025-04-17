using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace Infrastructure.ImplementationContract;

/// <summary>
/// Provides functionality to optimize images by converting them into the WebP format.
/// This class utilizes ImageSharp library to handle image manipulation and WebP encoding.
/// </summary>
public class ImageOptimizer : IImageOptimizer
{
    /// <summary>
    /// Optimizes the given image data by converting it to the WebP format with reduced quality.
    /// This reduces the image size while attempting to preserve quality.
    /// </summary>
    /// <param name="imageData">The raw byte array representing the image to be optimized.</param>
    /// <returns>
    /// A byte array containing the optimized image in WebP format.
    /// </returns>
    public async Task<byte[]> OptimizeImageAsync(byte[] imageData)
    {
        // Load the image from the provided byte array
        using var image = Image.Load(imageData);

        // Create a WebP encoder with a quality setting of 80 (out of 100)
        WebpEncoder webpOptions = new()
        {
            Quality = 80
        };

        // Initialize a memory stream to save the optimized image
        using MemoryStream ms = new();

        // Save the image in the WebP format to the memory stream
        await image.SaveAsync(ms, webpOptions);

        // Return the byte array representing the optimized image
        return ms.ToArray();
    }
}