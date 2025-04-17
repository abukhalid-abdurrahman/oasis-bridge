namespace Application.Contracts;

public interface IImageOptimizer
{
    Task<byte[]> OptimizeImageAsync(byte[] imageData);
}