namespace Infrastructure.ImplementationContract;

public sealed class IpfsService(
    ILogger<IpfsService> logger,
    IDecentralizedFileStorage fileStorage) : IIpfsService
{
    public async Task<Result<FileUploadResponse>> UploadFileAsync(FileUploadRequest request,
        CancellationToken token = default)
    {
        DateTimeOffset date = DateTimeOffset.UtcNow;
        logger.OperationStarted(nameof(UploadFileAsync), date);

        if (request.File.Length == 0)
            return Result<FileUploadResponse>.Failure(ResultPatternError.BadRequest(Messages.IpfsUploadEmptyFile));

        if (!Enum.IsDefined(typeof(FileType), request.Type))
            return Result<FileUploadResponse>.Failure(
                ResultPatternError.UnsupportedMediaType(Messages.IpfsInvalidTypeFile));

        try
        {
            await using Stream stream = request.File.OpenReadStream();
            string fileHash = await fileStorage.CreateAsync(stream, request.File.FileName, token);
            string fileUrl = $"https://ipfs.io/ipfs/{fileHash}";

            logger.OperationCompleted(nameof(UploadFileAsync), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
            return Result<FileUploadResponse>.Success(
                new(Messages.IpfsSuccessMessageFile,
                    new(fileHash, fileUrl)));
        }
        catch (Exception ex)
        {
            logger.OperationException(nameof(UploadFileAsync), ex.Message);
            logger.OperationCompleted(nameof(UploadFileAsync), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
            return Result<FileUploadResponse>.Failure(ResultPatternError.InternalServerError(ex.Message));
        }
    }
}