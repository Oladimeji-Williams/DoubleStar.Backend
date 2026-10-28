// Actor names stay null on the public-facing lists; only the admin "all reviews" view resolves them.
using DoubleStar.Modules.Reviews.Application.DTOs;
using DoubleStar.Modules.Reviews.Domain.Entities;

namespace DoubleStar.Modules.Reviews.Application.Mappings;

public static class ReviewMappings
{
    public static ReviewDto ToDto(this Review review, bool canDelete) => new(
        review.Id, review.ReviewerName, review.IsAnonymous, review.Rating, review.Comment,
        review.Status.ToString(), review.CreatedAt, canDelete,
        review.IsDeleted, review.DeletedAt, null,
        review.ApprovedAtUtc, null,
        review.RejectedAtUtc, null);
}