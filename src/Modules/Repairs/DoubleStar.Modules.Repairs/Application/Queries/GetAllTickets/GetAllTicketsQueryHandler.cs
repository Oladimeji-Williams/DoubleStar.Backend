// .../GetAllTicketsQueryHandler.cs
using DoubleStar.Modules.Repairs.Application.Abstractions;
using DoubleStar.Modules.Repairs.Application.DTOs;
using DoubleStar.Modules.Repairs.Application.Mappings;

namespace DoubleStar.Modules.Repairs.Application.Queries.GetAllTicketsQuery;

public sealed class GetAllTicketsQueryHandler(IRepairTicketRepository repairTicketRepository)
    : IRequestHandler<GetAllTicketsQuery, Result<IReadOnlyList<RepairTicketDto>>>
{
    public async Task<Result<IReadOnlyList<RepairTicketDto>>> Handle(
        GetAllTicketsQuery request, CancellationToken cancellationToken)
    {
        var tickets = await repairTicketRepository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<RepairTicketDto>>.Success(tickets.Select(t => t.ToDto()).ToList());
    }
}