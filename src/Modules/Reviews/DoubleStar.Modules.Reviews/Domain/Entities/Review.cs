// Modules/Reviews/Domain/Entities/Review.cs
using DoubleStar.SharedKernel.Domain;
using DoubleStar.Modules.Reviews.Domain.Enums;

namespace DoubleStar.Modules.Reviews.Domain.Entities;

public sealed class Review : GuidEntity
{
    public Guid? ReviewerUserId { get; private set; }
    public string ReviewerName { get; private set; } = "Anonymous";
    public int Rating { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public ReviewStatus Status { get; private set; }

    public bool IsAnonymous => ReviewerUserId is null;

    private Review() { }

    public static Review Create(Guid? reviewerUserId, string? reviewerName, int rating, string comment)
    {
        if (rating is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5.");
        }

        return new Review
        {
            Id = Guid.NewGuid(),
            ReviewerUserId = reviewerUserId,
            ReviewerName = reviewerUserId is not null ? (reviewerName?.Trim() is { Length: > 0 } name ? name : "Customer") : "Anonymous",
            Rating = rating,
            Comment = comment.Trim(),
            Status = ReviewStatus.Pending,
        };
    }

    public void Approve() => Status = ReviewStatus.Approved;
    public void Reject() => Status = ReviewStatus.Rejected;
}