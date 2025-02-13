namespace BuildingBlocks.Extensions.ResultPattern;

public sealed class Result<T> : BaseResult
{
    public T? Value { get; init; }

    private Result(bool isSuccess, ResultPatternError error, T? value) : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Success(T? value) => new(true, ResultPatternError.None(), value);

    public new static Result<T> Failure(ResultPatternError error) => new(false, error, default);
}