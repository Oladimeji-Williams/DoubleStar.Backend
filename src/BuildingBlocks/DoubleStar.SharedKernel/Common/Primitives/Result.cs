// Common/Primitives/Result.cs
namespace DoubleStar.SharedKernel.Common.Primitives;

public sealed class Result
{
    private Result(IReadOnlyList<Error> errors) => Errors = errors;

    public IReadOnlyList<Error> Errors { get; }
    public bool IsSuccess => Errors.Count == 0;
    public bool IsFailure => !IsSuccess;

    public static Result Success() => new([]);

    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result([error]);
    }

    public static Result Failure(IEnumerable<Error> errors)
    {
        var list = errors.ToList();
        if (list.Count == 0)
        {
            throw new ArgumentException("At least one error is required.", nameof(errors));
        }
        return new Result(list);
    }
}