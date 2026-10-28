using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Reviews.Domain.Entities;

namespace DoubleStar.Modules.Reviews.Application.Abstractions;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Review>> GetApprovedAsync(
        CancellationToken cancellationToken = default);

    Task<PagedResult<Review>> GetPendingPagedAsync(
        PagedQuery query,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Review>> GetAllPagedAsync(
        PagedQuery query,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Review review,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Review review,
        CancellationToken cancellationToken = default);
}