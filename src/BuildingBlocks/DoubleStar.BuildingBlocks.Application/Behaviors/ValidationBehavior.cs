// Behaviors/ValidationBehavior.cs
using FluentValidation;
using MediatR;
using DoubleStar.SharedKernel.Common.Primitives;

namespace DoubleStar.BuildingBlocks.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errors = results
            .SelectMany(r => r.Errors)
            .Select(e => new Error($"Validation.{e.PropertyName}", e.ErrorMessage, ErrorType.Validation))
            .ToList();

        if (errors.Count == 0)
        {
            return await next(cancellationToken);
        }

        return CreateFailureResponse(errors);
    }

    private static TResponse CreateFailureResponse(IReadOnlyList<Error> errors)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(errors);
        }

        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = typeof(TResponse).GetGenericArguments()[0];
            var resultType = typeof(Result<>).MakeGenericType(valueType);
            var failureMethod = resultType.GetMethod(nameof(Result<object>.Failure), [typeof(IEnumerable<Error>)]);
            return (TResponse)failureMethod!.Invoke(null, [errors])!;
        }

        throw new InvalidOperationException(
            $"ValidationBehavior cannot handle response type {typeof(TResponse).Name}. " +
            "MediatR requests using this behavior must return Result or Result<T>.");
    }
}