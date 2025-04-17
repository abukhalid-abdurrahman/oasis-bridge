namespace API.Controllers.V1;

[Route($"{ApiAddress.Base}/files")]
[AllowAnonymous]
public class IpfsController(IIpfsService service) : V1BaseController
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
}