namespace BuildingBlocks.Extensions.ResultPattern;

/// <summary>
/// Represents a standardized error result used in the result pattern.
/// Encapsulates the HTTP-style error <see cref="Code"/>, a descriptive <see cref="Message"/>,
/// and an <see cref="ErrorType"/> indicating the nature of the error.
/// </summary>
public sealed record ResultPatternError
{
    /// <summary>
    /// Gets the error code, typically aligned with HTTP status codes (e.g. 404, 500).
    /// </summary>
    public HttpStatusCode? Code { get; init; }

    /// <summary>
    /// Gets a human-readable description of the error.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Gets the categorization of the error type.
    /// </summary>
    public ErrorType ErrorType { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="ResultPatternError"/> with a default internal server error.
    /// </summary>
    private ResultPatternError()
    {
        Code = HttpStatusCode.InternalServerError;
        Message = "Internal server error!";
        ErrorType = ErrorType.InternalServerError;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResultPatternError"/> class with custom values.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The descriptive message for the error.</param>
    /// <param name="errorType">The error category.</param>
    private ResultPatternError(HttpStatusCode? code, string? message, ErrorType errorType)
    {
        Code = code;
        Message = message;
        ErrorType = errorType;
    }

    /// <summary>
    /// Creates a successful result with code 200 and optional message.
    /// </summary>
    /// <param name="message">The success message. Default is "Ok".</param>
    /// <returns>A <see cref="ResultPatternError"/> instance representing success.</returns>
    public static ResultPatternError None(string message = "Ok")
        => new(HttpStatusCode.OK, message, ErrorType.None);

    /// <summary>
    /// Creates a result indicating that the requested data was not found (404).
    /// </summary>
    /// <param name="message">The error message. Default is "Data not found!".</param>
    /// <returns>A <see cref="ResultPatternError"/> representing a not found error.</returns>
    public static ResultPatternError NotFound(string? message = "Data not found!")
        => new(HttpStatusCode.NotFound, message, ErrorType.NotFound);

    /// <summary>
    /// Creates a result indicating a bad request (400).
    /// </summary>
    /// <param name="message">The error message. Default is "Bad request!".</param>
    /// <returns>A <see cref="ResultPatternError"/> representing a bad request.</returns>
    public static ResultPatternError BadRequest(string? message = "Bad request!")
        => new(HttpStatusCode.BadRequest, message, ErrorType.BadRequest);

    /// <summary>
    /// Creates a result indicating that the resource already exists (409).
    /// </summary>
    /// <param name="message">The error message. Default is "Already exist!".</param>
    /// <returns>A <see cref="ResultPatternError"/> representing a resource conflict.</returns>
    public static ResultPatternError AlreadyExist(string? message = "Already exist!")
        => new(HttpStatusCode.Conflict, message, ErrorType.AlreadyExist);

    /// <summary>
    /// Creates a result indicating a conflict error (409).
    /// </summary>
    /// <param name="message">The error message. Default is "Conflict!".</param>
    /// <returns>A <see cref="ResultPatternError"/> representing a conflict error.</returns>
    public static ResultPatternError Conflict(string? message = "Conflict!")
        => new(HttpStatusCode.Conflict, message, ErrorType.Conflict);

    /// <summary>
    /// Creates a result indicating an internal server error (500).
    /// </summary>
    /// <param name="message">The error message. Default is "Internal server error!".</param>
    /// <returns>A <see cref="ResultPatternError"/> representing a server error.</returns>
    public static ResultPatternError InternalServerError(string? message = "Internal server error!")
        => new(HttpStatusCode.InternalServerError, message, ErrorType.InternalServerError);

    public static ResultPatternError UnsupportedMediaType(string? message = "Unsupported Media Type!")
        => new(HttpStatusCode.UnsupportedMediaType, message, ErrorType.UnsupportedMediaType);
}