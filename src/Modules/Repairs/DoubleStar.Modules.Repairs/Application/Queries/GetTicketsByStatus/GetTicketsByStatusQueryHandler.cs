// .../GetTicketsByStatusQueryHandler.cs
using DoubleStar.Modules.Repairs.Application.Abstractions;
using DoubleStar.Modules.Repairs.Application.DTOs;
using DoubleStar.Modules.Repairs.Application.Mappings;

namespace DoubleStar.Modules.Repairs.Application.Queries.GetTicketsByStatusQuery;

public sealed class GetTicketsByStatusQueryHandler(IRepairTicketRepository repairTicketRepository)
    : IRequestHandler<GetTicketsByStatusQuery, Result<IReadOnlyList<RepairTicketDto>>>
{
    public async Task<Result<IReadOnlyList<RepairTicketDto>>> Handle(
        GetTicketsByStatusQuery request, CancellationToken cancellationToken)
    {
        var tickets = await repairTicketRepository.GetByStatusAsync(request.Status, cancellationToken);
        return Result<IReadOnlyList<RepairTicketDto>>.Success(tickets.Select(t => t.ToDto()).ToList());
    }
}