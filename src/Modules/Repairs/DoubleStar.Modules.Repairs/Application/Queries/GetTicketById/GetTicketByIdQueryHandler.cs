// Application/Queries/GetTicketByIdQuery/GetTicketByIdQueryHandler.cs — full replacement
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.Modules.Repairs.Application.Abstractions;
using DoubleStar.Modules.Repairs.Application.DTOs;
using DoubleStar.Modules.Repairs.Application.Errors;
using DoubleStar.Modules.Repairs.Application.Mappings;

namespace DoubleStar.Modules.Repairs.Application.Queries.GetTicketByIdQuery;

public sealed class GetTicketByIdQueryHandler(IRepairTicketRepository repairTicketRepository, IProductCatalog productCatalog)
    : IRequestHandler<GetTicketByIdQuery, Result<RepairTicketDto>>
{
    public async Task<Result<RepairTicketDto>> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await repairTicketRepository.GetByIdAsync(request.Id, cancellationToken);
        if (ticket is null)
        {
            return Result<RepairTicketDto>.Failure(RepairErrors.NotFound(request.Id));
        }

        var dto = await ticket.ToDto().WithProductNamesAsync(productCatalog, cancellationToken);
        return Result<RepairTicketDto>.Success(dto);
    }
}