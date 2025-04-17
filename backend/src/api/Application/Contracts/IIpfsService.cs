namespace Application.Contracts;

public interface IIpfsService
{
    Task<Result<FileUploadResponse>> UploadFileAsync(FileUploadRequest request, CancellationToken token = default);
    Task<Result<byte[]>> GetFileAsync(string cid, CancellationToken token = default);
}