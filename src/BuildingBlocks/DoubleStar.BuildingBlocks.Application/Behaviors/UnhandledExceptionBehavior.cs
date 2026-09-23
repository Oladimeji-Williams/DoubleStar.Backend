// Behaviors/UnhandledExceptionBehavior.cs
using MediatR;
using Microsoft.Extensions.Logging;
using DoubleStar.SharedKernel.Common.Primitives;

namespace DoubleStar.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Last line of defense: turns an unexpected exception into a Result.Failure
/// instead of an unhandled 500. Only applies to handlers returning Result or
/// Result&lt;T&gt; — anything else propagates the original exception untouched.
/// </summary>
public sealed class UnhandledExceptionBehavior<TRequest, TResponse>(
    ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException && IsResultShaped())
        {
            logger.LogError(ex, "Unhandled exception processing {RequestName}", typeof(TRequest).Name);
            var error = new Error("Unexpected.Failure", "An unexpected error occurred. Please try again.", ErrorType.Failure);
            return CreateFailureResponse(error);
        }
    }

    private static bool IsResultShaped() =>
        typeof(TResponse) == typeof(Result) ||
        (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>));

    private static TResponse CreateFailureResponse(Error error)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        var valueType = typeof(TResponse).GetGenericArguments()[0];
        var resultType = typeof(Result<>).MakeGenericType(valueType);
        var failureMethod = resultType.GetMethod(nameof(Result<object>.Failure), [typeof(Error)]);
        return (TResponse)failureMethod!.Invoke(null, [error])!;
    }
}