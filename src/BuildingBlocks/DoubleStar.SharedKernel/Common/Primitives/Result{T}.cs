// Common/Primitives/Result{T}.cs
namespace DoubleStar.SharedKernel.Common.Primitives;

public sealed class Result<T>
{
    private Result(T? value, IReadOnlyList<Error> errors)
    {
        Value = value;
        Errors = errors;
    }

    public T? Value { get; }
    public IReadOnlyList<Error> Errors { get; }
    public bool IsSuccess => Errors.Count == 0;
    public bool IsFailure => !IsSuccess;

    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Result<T>(value, []);
    }

    public static Result<T> Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result<T>(default, [error]);
    }

    public static Result<T> Failure(IEnumerable<Error> errors)
    {
        var list = errors.ToList();
        if (list.Count == 0)
        {
            throw new ArgumentException("At least one error is required.", nameof(errors));
        }
        return new Result<T>(default, list);
    }
}