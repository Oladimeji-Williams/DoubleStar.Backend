using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Orders.Domain.Entities;

namespace DoubleStar.Modules.Orders.Application.Abstractions;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Order?> GetByIdempotencyKeyAsync(Guid customerId, string key, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> GetPendingExpiredBeforeAsync(
        DateTime cutoffUtc, int take, CancellationToken cancellationToken = default);

    Task<PagedResult<Order>> SearchAsync(PagedQuery query, CancellationToken cancellationToken = default);

    Task<PagedResult<Order>> SearchForCustomerAsync(
        Guid customerId, PagedQuery query, CancellationToken cancellationToken = default);

    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    Task SaveAsync(Order order, CancellationToken cancellationToken = default);
}