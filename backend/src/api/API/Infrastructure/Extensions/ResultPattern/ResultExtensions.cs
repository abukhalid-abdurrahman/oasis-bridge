namespace API.Infrastructure.Extensions.ResultPattern;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        ApiResponse<T> apiResponse = result.IsSuccess
            ? ApiResponse<T>.Success(result.Value)
            : ApiResponse<T>.Fail(result.Error, result.Value);

        return result.Error.ErrorType switch
        {
            ErrorType.Conflict => new ConflictObjectResult(apiResponse),
            ErrorType.AlreadyExist => new ConflictObjectResult(apiResponse),
            ErrorType.NotFound => new NotFoundObjectResult(apiResponse),
            ErrorType.BadRequest => new BadRequestObjectResult(apiResponse),
            ErrorType.None => new OkObjectResult(apiResponse),
            _ => new ObjectResult(apiResponse) { StatusCode = 500 }
        };
    }

    public static IActionResult ToActionResult(this BaseResult result)
    {
        ApiResponse<BaseResult> apiResponse = result.IsSuccess
            ? ApiResponse<BaseResult>.Success(null)
            : ApiResponse<BaseResult>.Fail(result.Error);

        return result.Error.ErrorType switch
        {
            ErrorType.Conflict => new ConflictObjectResult(apiResponse),
            ErrorType.AlreadyExist => new ConflictObjectResult(apiResponse),
            ErrorType.NotFound => new NotFoundObjectResult(apiResponse),
            ErrorType.BadRequest => new BadRequestObjectResult(apiResponse),
            ErrorType.None => new OkObjectResult(apiResponse),
            _ => new ObjectResult(apiResponse) { StatusCode = 500 }
        };
    }
}