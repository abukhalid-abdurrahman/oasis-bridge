namespace API.Controllers.Base;

[Route("/error")]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController(ILogger<ErrorController> logger) : BaseController
{
    [HttpGet, HttpPost, HttpPut, HttpDelete, HttpPatch, HttpOptions, HttpHead]
    public IActionResult HandleError()
    {
        Exception? exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

        if (exception != null)
            logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        return exception switch
        {
            ValidationException validationEx => Problem(
                title: "Validation exception!",
                detail: validationEx.Message,
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path,
                type: exception.HelpLink
            ),
            UnauthorizedAccessException => Problem(
                title: "Unauthorized access!",
                detail: "You are not authorized to access this resource.",
                statusCode: StatusCodes.Status401Unauthorized,
                instance: HttpContext.Request.Path,
                type: "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/401"
            ),
            OperationCanceledException => Problem(
                title: "Request canceled",
                detail: "The request was canceled by the client.",
                statusCode: StatusCodes.Status499ClientClosedRequest,
                instance: HttpContext.Request.Path,
                type: "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/499"
            ),
            _ => Problem(
                title: "An unexpected error occurred",
                detail: "Something went wrong. Please try again later.",
                statusCode: StatusCodes.Status500InternalServerError,
                instance: HttpContext.Request.Path,
                type: "https://learn.microsoft.com/"
            )
        };
    }
}