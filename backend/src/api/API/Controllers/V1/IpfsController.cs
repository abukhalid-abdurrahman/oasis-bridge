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
}