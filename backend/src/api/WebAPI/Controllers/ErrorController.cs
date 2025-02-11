namespace WebAPI.Controllers;

[Route("/error")]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : BaseController
{
    [HttpGet, HttpPost, HttpPut, HttpDelete, HttpPatch, HttpOptions, HttpHead]
    public IActionResult HandleError()
    {
        Exception? exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

        return exception switch
        {
            ValidationException => Problem(
                title: "Validation exception!",
                detail: exception?.Message,
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path,
                type: exception?.HelpLink
            ),
            _ => Problem(
                title: "An unexpected error occurred",
                detail: exception?.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                instance: HttpContext.Request.Path,
                type: "https://metanit.com"
            )
        };
    }
}