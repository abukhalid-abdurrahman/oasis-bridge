namespace API.Controllers.V1;

[Route($"{ApiAddress.Base}/files")]
[AllowAnonymous]
public class IpfsController(IIpfsService service,IImageOptimizer imageOptimizer) : V1BaseController
{
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAsync(
        [FromForm] FileUploadRequest request,
        CancellationToken token)
        => (await service.UploadFileAsync(request, token)).ToActionResult();

    [HttpGet("full/{cid:required}")]
    public async Task<IActionResult> GetAsync(string cid, CancellationToken token)
    {
        Result<byte[]> result = await service.GetFileAsync(cid, token);

        if (!result.IsSuccess)
            return result.ToActionResult();

        return File(result.Value!, "application/octet-stream", cid);
    }

    [HttpGet("nft-logo/{fileId:required}/optimized")]
    public async Task<IActionResult> GetOptimizedNftLogoAsync(string fileId, CancellationToken token)
    {
        Result<byte[]> result = await service.GetFileAsync(fileId, token);

        if (!result.IsSuccess)
            return result.ToActionResult();

        try
        {
            byte[] optimizedImage = await imageOptimizer.OptimizeImageAsync(result.Value!);

            string contentType = "image/webp";

            Response.Headers["Cache-Control"] = "public, max-age=604800";

            return File(optimizedImage, contentType);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}