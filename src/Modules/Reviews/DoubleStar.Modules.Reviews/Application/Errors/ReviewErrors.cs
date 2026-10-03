// Modules/Reviews/Application/Errors/ReviewErrors.cs
using DoubleStar.SharedKernel.Common.Primitives;

namespace DoubleStar.Modules.Reviews.Application.Errors;

public static class ReviewErrors
{
    public static Error NotFound(Guid id) => new("Review.NotFound", $"Review '{id}' was not found.", ErrorType.NotFound);
    public static Error NotAuthorizedToDelete() => new("Review.NotAuthorizedToDelete", "You can only delete your own reviews.", ErrorType.Forbidden);
}