// .../SubmitReviewCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.Modules.Reviews.Application.Abstractions;
using DoubleStar.Modules.Reviews.Application.DTOs;
using DoubleStar.Modules.Reviews.Application.Mappings;
using DoubleStar.Modules.Reviews.Domain.Entities;

namespace DoubleStar.Modules.Reviews.Application.Commands.SubmitReviewCommand;

public sealed class SubmitReviewCommandHandler(
    IReviewRepository reviewRepository, ICurrentUser currentUser, IIdentityService identityService)
    : IRequestHandler<SubmitReviewCommand, Result<ReviewDto>>
{
    public async Task<Result<ReviewDto>> Handle(SubmitReviewCommand request, CancellationToken cancellationToken)
    {
        string? reviewerName = null;

        if (currentUser.UserId is not null)
        {
            var profile = await identityService.GetProfileAsync(currentUser.UserId.Value, cancellationToken);
            reviewerName = profile is not null ? $"{profile.FirstName} {profile.LastName}".Trim() : null;
        }

        var review = Review.Create(currentUser.UserId, reviewerName, request.Rating, request.Comment);
        await reviewRepository.AddAsync(review, cancellationToken);

        // Starts Pending for every reviewer, anonymous or not — a staff member must approve it before it's public.
        return Result<ReviewDto>.Success(review.ToDto(canDelete: true));
    }
}