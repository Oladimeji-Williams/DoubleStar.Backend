// .../GetTicketByIdQueryHandler.cs
using DoubleStar.Modules.Repairs.Application.Abstractions;
using DoubleStar.Modules.Repairs.Application.DTOs;
using DoubleStar.Modules.Repairs.Application.Errors;
using DoubleStar.Modules.Repairs.Application.Mappings;

namespace DoubleStar.Modules.Repairs.Application.Queries.GetTicketByIdQuery;

public sealed class GetTicketByIdQueryHandler(IRepairTicketRepository repairTicketRepository)
    : IRequestHandler<GetTicketByIdQuery, Result<RepairTicketDto>>
{
    public async Task<Result<RepairTicketDto>> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await repairTicketRepository.GetByIdAsync(request.Id, cancellationToken);
        return ticket is null
            ? Result<RepairTicketDto>.Failure(RepairErrors.NotFound(request.Id))
            : Result<RepairTicketDto>.Success(ticket.ToDto());
    }
}