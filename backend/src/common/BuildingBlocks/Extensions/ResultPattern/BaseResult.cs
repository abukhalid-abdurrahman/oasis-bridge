namespace BuildingBlocks.Extensions.ResultPattern;

public class BaseResult
{
    public bool IsSuccess { get; init; }
    public ResultPatternError Error { get; init; }

    protected BaseResult(bool isSuccess, ResultPatternError error)
    {
        Error = error;
        IsSuccess = isSuccess;
    }

    public static BaseResult Success(string message="Ok") => new(true, ResultPatternError.None(message));

    public static BaseResult Failure(ResultPatternError error) => new(false, error);
}