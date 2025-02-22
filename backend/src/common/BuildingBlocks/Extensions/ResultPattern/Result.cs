namespace BuildingBlocks.Extensions.ResultPattern;

public sealed class Result<T> : BaseResult
{
    public T? Value { get; init; }

    private Result(bool isSuccess, ResultPatternError error, T? value) : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Success(T? value) => new(true, ResultPatternError.None(), value);

    public static Result<T> Failure(ResultPatternError error, T value=default!) => new(false, error, value);
}