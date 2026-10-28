using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Repairs.Application.Abstractions;
using DoubleStar.Modules.Repairs.Application.DTOs;
using DoubleStar.Modules.Repairs.Application.Mappings;
using DoubleStar.SharedKernel.Contracts.Catalog;

namespace DoubleStar.Modules.Repairs.Application.Queries.GetTicketsByCustomer;

public sealed class GetTicketsByCustomerQueryHandler(
    IRepairTicketRepository repairTicketRepository,
    IProductCatalog productCatalog)
    : IRequestHandler<
        GetTicketsByCustomerQuery,
        Result<PagedResult<RepairTicketDto>>>
{
    public async Task<Result<PagedResult<RepairTicketDto>>> Handle(
        GetTicketsByCustomerQuery request,
        CancellationToken cancellationToken)
    {
        var result =
            await repairTicketRepository.SearchPagedForCustomerAsync(
                request.CustomerId,
                request.Query,
                cancellationToken);

        var items = result.Items
            .Select(ticket => ticket.ToDto())
            .ToList();

        var enrichedItems =
            await items.WithProductNamesAsync(
                productCatalog,
                cancellationToken);

        return Result<PagedResult<RepairTicketDto>>.Success(
            new PagedResult<RepairTicketDto>(
                enrichedItems,
                result.Page,
                result.PageSize,
                result.TotalCount));
    }
}