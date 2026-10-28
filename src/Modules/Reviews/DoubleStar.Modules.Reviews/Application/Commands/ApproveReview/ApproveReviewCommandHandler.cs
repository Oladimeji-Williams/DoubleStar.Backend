using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.Modules.Reviews.Application.Abstractions;
using DoubleStar.Modules.Reviews.Application.Errors;

namespace DoubleStar.Modules.Reviews.Application.Commands.ApproveReviewCommand;

public sealed class ApproveReviewCommandHandler(IReviewRepository reviewRepository, ICurrentUser currentUser)
    : IRequestHandler<ApproveReviewCommand, Result>
{
    public async Task<Result> Handle(ApproveReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review is null) return Result.Failure(ReviewErrors.NotFound(request.ReviewId));

        review.Approve(currentUser.UserId!.Value);
        await reviewRepository.UpdateAsync(review, cancellationToken);
        return Result.Success();
    }
}