// Modules/Reviews/Persistence/Repositories/ReviewRepository.cs
using Microsoft.EntityFrameworkCore;
using DoubleStar.Modules.Reviews.Application.Abstractions;
using DoubleStar.Modules.Reviews.Domain.Entities;
using DoubleStar.Modules.Reviews.Domain.Enums;

namespace DoubleStar.Modules.Reviews.Persistence.Repositories;

internal sealed class ReviewRepository(ReviewsDbContext dbContext) : IReviewRepository
{
    public Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Review>> GetApprovedAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Reviews.Where(r => r.Status == ReviewStatus.Approved)
            .OrderByDescending(r => r.CreatedAt).Take(50).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Review>> GetPendingAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Reviews.Where(r => r.Status == ReviewStatus.Pending)
            .OrderBy(r => r.CreatedAt).ToListAsync(cancellationToken);

    public async Task AddAsync(Review review, CancellationToken cancellationToken = default)
    {
        await dbContext.Reviews.AddAsync(review, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Review review, CancellationToken cancellationToken = default)
    {
        dbContext.Reviews.Update(review);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}