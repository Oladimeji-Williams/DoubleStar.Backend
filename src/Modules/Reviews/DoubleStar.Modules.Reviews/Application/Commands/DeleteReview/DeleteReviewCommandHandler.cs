using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.Modules.Reviews.Application.Abstractions;
using DoubleStar.Modules.Reviews.Application.Errors;

namespace DoubleStar.Modules.Reviews.Application.Commands.DeleteReviewCommand;

public sealed class DeleteReviewCommandHandler(IReviewRepository reviewRepository, ICurrentUser currentUser)
    : IRequestHandler<DeleteReviewCommand, Result>
{
    public async Task<Result> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review is null) return Result.Failure(ReviewErrors.NotFound(request.ReviewId));

        var isOwner = review.ReviewerUserId is not null && review.ReviewerUserId == currentUser.UserId;

        // Scoped to Manager and Admin — a Cashier or Technician can no longer delete someone
        // else's review. Admin is included deliberately: locking an Admin out of something a
        // Manager can do would be an odd gap. Drop the Admin check below if you want this
        // genuinely Manager-only.
        var canModerate = currentUser.IsInRole("Manager") || currentUser.IsInRole("Admin");

        if (!canModerate && !isOwner)
        {
            return Result.Failure(ReviewErrors.NotAuthorizedToDelete());
        }

        review.DeleteBy(currentUser.UserId);
        await reviewRepository.UpdateAsync(review, cancellationToken);
        return Result.Success();
    }
}