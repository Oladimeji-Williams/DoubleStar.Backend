// Application/CrossModule/RepairHistoryService.cs
using DoubleStar.SharedKernel.Contracts.Repairs;
using DoubleStar.Modules.Repairs.Application.Abstractions;
using DoubleStar.Modules.Repairs.Application.Mappings;

namespace DoubleStar.Modules.Repairs.Application.CrossModule;

public sealed class RepairHistoryService(IRepairTicketRepository repairTicketRepository) : IRepairHistory
{
    public async Task<IReadOnlyList<RepairTicketSummaryDto>> GetAllForCustomerAsync(
        Guid customerId, CancellationToken cancellationToken = default)
    {
        var tickets = await repairTicketRepository.GetAllForCustomerAsync(customerId, cancellationToken);
        return tickets.Select(t => t.ToSummaryDto()).ToList();
    }

    public async Task<RepairTicketSummaryDto?> GetByIdAsync(int ticketId, CancellationToken cancellationToken = default)
    {
        var ticket = await repairTicketRepository.GetByIdAsync(ticketId, cancellationToken);
        return ticket?.ToSummaryDto();
    }

    public async Task<IReadOnlyList<RepairTicketSummaryDto>> GetCollectedInRangeAsync(
        DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var tickets = await repairTicketRepository.GetCollectedInRangeAsync(fromUtc, toUtc, cancellationToken);
        return tickets.Select(t => t.ToSummaryDto()).ToList();
    }
}