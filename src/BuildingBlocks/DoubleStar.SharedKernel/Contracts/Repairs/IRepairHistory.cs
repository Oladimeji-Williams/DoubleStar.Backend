// Contracts/Repairs/IRepairHistory.cs
namespace DoubleStar.SharedKernel.Contracts.Repairs;

public interface IRepairHistory
{
    Task<IReadOnlyList<RepairTicketSummaryDto>> GetAllForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<RepairTicketSummaryDto?> GetByIdAsync(int ticketId, CancellationToken cancellationToken = default);
}