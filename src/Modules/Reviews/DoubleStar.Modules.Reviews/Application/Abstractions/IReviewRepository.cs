// Modules/Reviews/Application/Abstractions/IReviewRepository.cs
using DoubleStar.Modules.Reviews.Domain.Entities;

namespace DoubleStar.Modules.Reviews.Application.Abstractions;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Review>> GetApprovedAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Review>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Review review, CancellationToken cancellationToken = default);
    Task UpdateAsync(Review review, CancellationToken cancellationToken = default);
}