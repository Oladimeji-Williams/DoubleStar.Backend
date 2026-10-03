// Application/Queries/GetTicketsByCustomerQuery/GetTicketsByCustomerQueryHandler.cs — full replacement
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.Modules.Repairs.Application.Abstractions;
using DoubleStar.Modules.Repairs.Application.DTOs;
using DoubleStar.Modules.Repairs.Application.Mappings;

namespace DoubleStar.Modules.Repairs.Application.Queries.GetTicketsByCustomerQuery;

public sealed class GetTicketsByCustomerQueryHandler(IRepairTicketRepository repairTicketRepository, IProductCatalog productCatalog)
    : IRequestHandler<GetTicketsByCustomerQuery, Result<IReadOnlyList<RepairTicketDto>>>
{
    public async Task<Result<IReadOnlyList<RepairTicketDto>>> Handle(
        GetTicketsByCustomerQuery request, CancellationToken cancellationToken)
    {
        var tickets = await repairTicketRepository.GetAllForCustomerAsync(request.CustomerId, cancellationToken);
        var dtos = await tickets.Select(t => t.ToDto()).ToList().WithProductNamesAsync(productCatalog, cancellationToken);
        return Result<IReadOnlyList<RepairTicketDto>>.Success(dtos);
    }
}