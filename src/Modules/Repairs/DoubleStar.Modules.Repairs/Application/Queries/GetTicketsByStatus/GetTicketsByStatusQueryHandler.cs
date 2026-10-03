// Application/Queries/GetTicketsByStatusQuery/GetTicketsByStatusQueryHandler.cs — full replacement
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.Modules.Repairs.Application.Abstractions;
using DoubleStar.Modules.Repairs.Application.DTOs;
using DoubleStar.Modules.Repairs.Application.Mappings;

namespace DoubleStar.Modules.Repairs.Application.Queries.GetTicketsByStatusQuery;

public sealed class GetTicketsByStatusQueryHandler(IRepairTicketRepository repairTicketRepository, IProductCatalog productCatalog)
    : IRequestHandler<GetTicketsByStatusQuery, Result<IReadOnlyList<RepairTicketDto>>>
{
    public async Task<Result<IReadOnlyList<RepairTicketDto>>> Handle(
        GetTicketsByStatusQuery request, CancellationToken cancellationToken)
    {
        var tickets = await repairTicketRepository.GetByStatusAsync(request.Status, cancellationToken);
        var dtos = await tickets.Select(t => t.ToDto()).ToList().WithProductNamesAsync(productCatalog, cancellationToken);
        return Result<IReadOnlyList<RepairTicketDto>>.Success(dtos);
    }
}