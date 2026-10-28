using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Repairs.Domain.Entities;
using DoubleStar.SharedKernel.Contracts.Repairs;

namespace DoubleStar.Modules.Repairs.Application.Abstractions;

public interface IRepairTicketRepository
{
    Task<RepairTicket?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RepairTicket>> GetByStatusAsync(
        RepairStatus status,
        CancellationToken cancellationToken = default);

    Task<PagedResult<RepairTicket>> SearchPagedAsync(
        PagedQuery query,
        CancellationToken cancellationToken = default);

    Task<PagedResult<RepairTicket>> SearchPagedForCustomerAsync(
        Guid customerId,
        PagedQuery query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RepairTicket>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RepairTicket>> GetAllForCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RepairTicket>> GetCollectedInRangeAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        RepairTicket ticket,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        RepairTicket ticket,
        CancellationToken cancellationToken = default);
}