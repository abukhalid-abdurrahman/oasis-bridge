using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace Infrastructure.ImplementationContract;

public class ImageOptimizer : IImageOptimizer
{
    public async Task<byte[]> OptimizeImageAsync(byte[] imageData)
    {
        using var image = Image.Load(imageData);

        WebpEncoder webpOptions = new()
        {
            Quality = 80
        };

        using MemoryStream ms = new();
        await image.SaveAsync(ms, webpOptions);
        return ms.ToArray();
    }
}