namespace Application.Extensions.Responses.ApiResponse;

public class ApiResponse<T>
{
    public bool IsSuccess { get; init; }
    public ResultPatternError Error { get; init; }
    public T? Data { get; init; }

    private ApiResponse(bool isSuccess, ResultPatternError error, T? data)
    {
        IsSuccess = isSuccess;
        Error = error;
        Data = data;
    }

    public static ApiResponse<T> Success(T? data) => new(true, ResultPatternError.None(), data);

    public static ApiResponse<T> Fail(ResultPatternError error) => new(false, error, default);
    public static ApiResponse<T> Fail(ResultPatternError error,T? value) => new(false, error, value);
}