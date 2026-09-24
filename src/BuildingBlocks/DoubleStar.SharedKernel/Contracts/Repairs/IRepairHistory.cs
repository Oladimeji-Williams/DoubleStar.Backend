// SharedKernel/Contracts/Repairs/IRepairHistory.cs — replace the whole file
namespace DoubleStar.SharedKernel.Contracts.Repairs;

public interface IRepairHistory
{
    Task<IReadOnlyList<RepairTicketSummaryDto>> GetAllForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<RepairTicketSummaryDto?> GetByIdAsync(int ticketId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RepairTicketSummaryDto>> GetCollectedInRangeAsync(
        DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
}