using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.Modules.Sales.Application.Abstractions;
using DoubleStar.Modules.Sales.Application.DTOs;
using DoubleStar.Modules.Sales.Application.Mappings;

namespace DoubleStar.Modules.Sales.Application.Queries.GetAllSalesQuery;

public sealed class GetAllSalesQueryHandler(
    ISaleRepository saleRepository,
    IProductCatalog productCatalog)
    : IRequestHandler<
        GetAllSalesQuery,
        Result<PagedResult<SaleDto>>>
{
    public async Task<Result<PagedResult<SaleDto>>> Handle(
        GetAllSalesQuery request,
        CancellationToken cancellationToken)
    {
        var result =
            await saleRepository.SearchAsync(
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