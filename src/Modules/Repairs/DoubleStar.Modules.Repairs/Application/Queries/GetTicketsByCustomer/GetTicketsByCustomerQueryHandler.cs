// .../GetTicketsByCustomerQueryHandler.cs
using DoubleStar.Modules.Repairs.Application.Abstractions;
using DoubleStar.Modules.Repairs.Application.DTOs;
using DoubleStar.Modules.Repairs.Application.Mappings;

namespace DoubleStar.Modules.Repairs.Application.Queries.GetTicketsByCustomerQuery;

public sealed class GetTicketsByCustomerQueryHandler(IRepairTicketRepository repairTicketRepository)
    : IRequestHandler<GetTicketsByCustomerQuery, Result<IReadOnlyList<RepairTicketDto>>>
{
    public async Task<Result<IReadOnlyList<RepairTicketDto>>> Handle(
        GetTicketsByCustomerQuery request, CancellationToken cancellationToken)
    {
        var tickets = await repairTicketRepository.GetAllForCustomerAsync(request.CustomerId, cancellationToken);
        return Result<IReadOnlyList<RepairTicketDto>>.Success(tickets.Select(t => t.ToDto()).ToList());
    }
}