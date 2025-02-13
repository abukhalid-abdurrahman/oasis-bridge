namespace BuildingBlocks.Extensions.ResultPattern;

public sealed record ResultPatternError
{
    public int? Code { get; init; }
    public string? Message { get; init; }
    public ErrorType ErrorType { get; init; }

    private ResultPatternError()
    {
        Code = 500;
        Message = "Internal server error...!";
        ErrorType = ErrorType.InternalServerError;
    }

    private ResultPatternError(int? code, string? message, ErrorType errorType)
    {
        Code = code;
        Message = message;
        ErrorType = errorType;
    }


    public static ResultPatternError None()
        => new(null, null, ErrorType.None);

    public static ResultPatternError NotFound(string? message = "Data not found!")
        => new(404, message, ErrorType.NotFound);

    public static ResultPatternError BadRequest(string? message = "Bad request!")
        => new(400, message, ErrorType.BadRequest);

    public static ResultPatternError AlreadyExist(string? message = "Already exist!")
        => new(409, message, ErrorType.AlreadyExist);

    public static ResultPatternError Conflict(string? message = "Conflict!")
        => new(409, message, ErrorType.Conflict);

    public static ResultPatternError InternalServerError(string? message = "Internal server error!")
        => new(500, message, ErrorType.InternalServerError);
  
}