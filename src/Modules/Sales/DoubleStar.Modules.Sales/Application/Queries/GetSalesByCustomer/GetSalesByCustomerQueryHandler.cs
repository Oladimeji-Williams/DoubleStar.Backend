using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.Modules.Sales.Application.Abstractions;
using DoubleStar.Modules.Sales.Application.DTOs;
using DoubleStar.Modules.Sales.Application.Mappings;

namespace DoubleStar.Modules.Sales.Application.Queries.GetSalesByCustomerQuery;

public sealed class GetSalesByCustomerQueryHandler(
    ISaleRepository saleRepository,
    IProductCatalog productCatalog)
    : IRequestHandler<
        GetSalesByCustomerQuery,
        Result<PagedResult<SaleDto>>>
{
    public async Task<Result<PagedResult<SaleDto>>> Handle(
        GetSalesByCustomerQuery request,
        CancellationToken cancellationToken)
    {
        var result =
            await saleRepository.SearchForCustomerAsync(
                request.CustomerId,
                request.Query,
                cancellationToken);

        var dtos =
            result.Items
                .Select(s => s.ToDto())
                .ToList();

        var enrichedDtos =
            await dtos.WithProductNamesAsync(
                productCatalog,
                cancellationToken);

        var pagedResult =
            new PagedResult<SaleDto>(
                enrichedDtos,
                result.Page,
                result.PageSize,
                result.TotalCount);

        return Result<PagedResult<SaleDto>>
            .Success(pagedResult);
    }
}