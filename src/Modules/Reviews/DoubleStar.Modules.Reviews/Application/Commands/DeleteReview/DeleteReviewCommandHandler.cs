// .../DeleteReviewCommandHandler.cs
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

        // Anonymous reviews have no owner to verify against, so only staff can remove one —
        // allowing deletion by "anyone who knows the ID" would let a stranger delete anyone's review.
        var isOwner = review.ReviewerUserId is not null && review.ReviewerUserId == currentUser.UserId;
        if (!currentUser.IsStaff && !isOwner)
        {
            return Result.Failure(ReviewErrors.NotAuthorizedToDelete());
        }

        review.Delete();
        await reviewRepository.UpdateAsync(review, cancellationToken);
        return Result.Success();
    }
}