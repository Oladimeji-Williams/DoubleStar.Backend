// Application/Abstractions/IRepairTicketRepository.cs
using DoubleStar.SharedKernel.Contracts.Repairs;
using DoubleStar.Modules.Repairs.Domain.Entities;

namespace DoubleStar.Modules.Repairs.Application.Abstractions;

public interface IRepairTicketRepository
{
    Task<RepairTicket?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RepairTicket>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RepairTicket>> GetAllForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RepairTicket>> GetByStatusAsync(RepairStatus status, CancellationToken cancellationToken = default);
    Task AddAsync(RepairTicket ticket, CancellationToken cancellationToken = default);
    Task UpdateAsync(RepairTicket ticket, CancellationToken cancellationToken = default);
}