using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Sales.Domain.Entities;

namespace DoubleStar.Modules.Sales.Application.Abstractions;

public interface ISaleRepository
{
    Task<Sale?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Sale>> SearchAsync(
        PagedQuery query,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Sale>> SearchForCustomerAsync(
        Guid customerId,
        PagedQuery query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Sale>> GetAllForCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Sale>> GetAllInRangeAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Sale sale,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Sale sale,
        CancellationToken cancellationToken = default);
}