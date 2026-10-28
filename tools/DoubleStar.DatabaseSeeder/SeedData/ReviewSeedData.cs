using Microsoft.EntityFrameworkCore;
using DoubleStar.Modules.Reviews.Domain.Entities;
using DoubleStar.Modules.Reviews.Persistence;

namespace DoubleStar.DatabaseSeeder.SeedData;

public static class ReviewSeedData
{
    public static async Task SeedAsync(
        ReviewsDbContext dbContext,
        Guid moderatorUserId)
    {
        if (await dbContext.Reviews.IgnoreQueryFilters().AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        var reviews = new List<(Review Review, DateTime CreatedAt)>
        {
            (
                CreateApproved(
                    5,
                    "Excellent service. My phone was repaired quickly and works perfectly again.",
                    moderatorUserId),
                now.AddDays(-1)
            ),

            (
                CreateApproved(
                    5,
                    "Very professional team. The staff explained the problem clearly before starting the repair.",
                    moderatorUserId),
                now.AddDays(-2)
            ),

            (
                CreateApproved(
                    4,
                    "Good service and friendly staff. The repair took a little longer than expected, but the result was good.",
                    moderatorUserId),
                now.AddDays(-3)
            ),

            (
                CreateApproved(
                    5,
                    "I bought a charger and screen protector here. Genuine products and great customer service.",
                    moderatorUserId),
                now.AddDays(-4)
            ),

            (
                CreateApproved(
                    4,
                    "My laptop repair was handled professionally. I would definitely recommend DoubleStar.",
                    moderatorUserId),
                now.AddDays(-5)
            ),

            (
                CreateApproved(
                    5,
                    "Fast, reliable and affordable. Very happy with the service.",
                    moderatorUserId),
                now.AddDays(-6)
            ),

            (
                CreateApproved(
                    3,
                    "The repair was successful, although I had to wait longer than expected.",
                    moderatorUserId),
                now.AddDays(-7)
            ),

            (
                CreateApproved(
                    5,
                    "Great experience from start to finish. The team was helpful and transparent.",
                    moderatorUserId),
                now.AddDays(-8)
            ),

            (
                CreateApproved(
                    4,
                    "Good customer service and reasonable pricing. My device is working well.",
                    moderatorUserId),
                now.AddDays(-9)
            ),

            (
                CreateApproved(
                    5,
                    "Very satisfied with the repair. I will definitely come back.",
                    moderatorUserId),
                now.AddDays(-10)
            ),

            (
                CreatePending(
                    5,
                    "Amazing service. The technician knew exactly what was wrong with my phone."),
                now.AddHours(-6)
            ),

            (
                CreatePending(
                    4,
                    "Good experience overall. The staff were polite and helpful."),
                now.AddHours(-12)
            ),

            (
                CreatePending(
                    3,
                    "The service was okay, but the waiting time could be improved."),
                now.AddDays(-1).AddHours(-4)
            ),

            (
                CreatePending(
                    5,
                    "Excellent repair service. Everything was handled professionally."),
                now.AddDays(-2).AddHours(-3)
            ),

            (
                CreatePending(
                    2,
                    "The repair eventually worked, but the process took longer than I expected."),
                now.AddDays(-3).AddHours(-2)
            ),

            (
                CreateRejected(
                    1,
                    "This review is intentionally seeded as rejected for moderation testing.",
                    moderatorUserId),
                now.AddDays(-4)
            ),

            (
                CreateRejected(
                    2,
                    "Another rejected review used to test the staff moderation table.",
                    moderatorUserId),
                now.AddDays(-5)
            ),

            (
                CreateApproved(
                    3,
                    "The staff were helpful and the device was repaired successfully.",
                    moderatorUserId),
                now.AddDays(-11)
            ),

            (
                CreateApproved(
                    5,
                    "Fantastic experience. I am impressed with the quality of service.",
                    moderatorUserId),
                now.AddDays(-12)
            ),

            (
                CreateApproved(
                    4,
                    "Professional service and good communication throughout the repair.",
                    moderatorUserId),
                now.AddDays(-13)
            )
        };

        // Add one deleted review for testing
        // IsDeleted filtering and IgnoreQueryFilters().
        var deletedReview = Review.Create(
            reviewerUserId: null,
            reviewerName: null,
            rating: 1,
            comment: "This review was deleted and is included for moderation testing.");

        deletedReview.DeleteBy(moderatorUserId);

        reviews.Add(
            (
                deletedReview,
                now.AddDays(-14)
            ));

        await dbContext.Reviews.AddRangeAsync(
            reviews.Select(x => x.Review));

        // Set historical audit dates after EF begins tracking the entities.
        foreach (var (review, createdAt) in reviews)
        {
            dbContext.Entry(review)
                .Property(nameof(Review.CreatedAt))
                .CurrentValue = createdAt;

            dbContext.Entry(review)
                .Property(nameof(Review.ModifiedAt))
                .CurrentValue = createdAt;
        }

        await dbContext.SaveChangesAsync();
    }

    private static Review CreateApproved(
        int rating,
        string comment,
        Guid moderatorUserId)
    {
        var review = Review.Create(
            reviewerUserId: null,
            reviewerName: null,
            rating,
            comment);

        review.Approve(moderatorUserId);

        return review;
    }

    private static Review CreatePending(
        int rating,
        string comment)
    {
        return Review.Create(
            reviewerUserId: null,
            reviewerName: null,
            rating,
            comment);
    }

    private static Review CreateRejected(
        int rating,
        string comment,
        Guid moderatorUserId)
    {
        var review = Review.Create(
            reviewerUserId: null,
            reviewerName: null,
            rating,
            comment);

        review.Reject(moderatorUserId);

        return review;
    }
}