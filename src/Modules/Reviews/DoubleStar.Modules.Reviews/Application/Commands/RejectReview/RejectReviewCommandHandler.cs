// .../RejectReviewCommandHandler.cs
using DoubleStar.Modules.Reviews.Application.Abstractions;
using DoubleStar.Modules.Reviews.Application.Errors;

namespace DoubleStar.Modules.Reviews.Application.Commands.RejectReviewCommand;

public sealed class RejectReviewCommandHandler(IReviewRepository reviewRepository)
    : IRequestHandler<RejectReviewCommand, Result>
{
    public async Task<Result> Handle(RejectReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review is null) return Result.Failure(ReviewErrors.NotFound(request.ReviewId));

        review.Reject();
        await reviewRepository.UpdateAsync(review, cancellationToken);
        return Result.Success();
    }
}