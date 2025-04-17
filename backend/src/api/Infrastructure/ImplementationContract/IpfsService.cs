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

    public async Task<Result<byte[]>> GetFileAsync(string cid, CancellationToken token = default)
    {
        DateTimeOffset started = DateTimeOffset.UtcNow;
        logger.OperationStarted(nameof(GetFileAsync), started);

        try
        {
            if (!IpfsValidator.IsValidCidV0(cid))
                return Result<byte[]>.Failure(ResultPatternError.BadRequest(Messages.IpfsInvalidFormatCid));

            IAsyncEnumerable<ReadOnlyMemory<byte>> fileChunks = fileStorage.GetAsync(cid, token: token);

            await using MemoryStream memoryStream = new();

            await foreach (ReadOnlyMemory<byte> chunk in fileChunks)
            {
                await memoryStream.WriteAsync(chunk, token);
            }

            if (memoryStream.Length == 0)
                return Result<byte[]>.Failure(ResultPatternError.NotFound(Messages.IpfsFileNotFound));

            byte[] resultBytes = memoryStream.ToArray();

            logger.OperationCompleted(nameof(GetFileAsync), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - started);
            return Result<byte[]>.Success(resultBytes);
        }
        catch (FileNotFoundException fnfEx)
        {
            logger.OperationException(nameof(GetFileAsync), fnfEx.Message);
            return Result<byte[]>.Failure(ResultPatternError.NotFound(fnfEx.Message));
        }
        catch (IOException ioEx)
        {
            logger.OperationException(nameof(GetFileAsync), ioEx.Message);
            return Result<byte[]>.Failure(ResultPatternError.InternalServerError(ioEx.Message));
        }
        catch (Exception ex)
        {
            logger.OperationException(nameof(GetFileAsync), ex.Message);
            return Result<byte[]>.Failure(ResultPatternError.InternalServerError(ex.Message));
        }
    }
}