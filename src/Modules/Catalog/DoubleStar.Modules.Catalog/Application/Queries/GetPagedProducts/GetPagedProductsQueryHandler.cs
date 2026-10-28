using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.SharedKernel.Contracts.Inventory;
using DoubleStar.Modules.Catalog.Application.Abstractions;
using DoubleStar.Modules.Catalog.Application.DTOs;
using DoubleStar.Modules.Catalog.Application.Mappings;

namespace DoubleStar.Modules.Catalog.Application.Queries.GetPagedProducts;

public sealed class GetPagedProductsQueryHandler(
    IProductRepository productRepository,
    IStockLevelReader stockLevelReader)
    : IRequestHandler<GetPagedProductsQuery, Result<PagedResult<ProductDto>>>
{
    public async Task<Result<PagedResult<ProductDto>>> Handle(
        GetPagedProductsQuery request, CancellationToken cancellationToken)
    {
        var result = await productRepository.SearchAsync(request.Query, request.IncludeArchived, cancellationToken);

        IReadOnlyList<ProductDto> items = result.Items.Select(product => product.ToDto()).ToList();
        items = await items.WithAvailabilityAsync(stockLevelReader, cancellationToken);

        return Result<PagedResult<ProductDto>>.Success(
            new PagedResult<ProductDto>(items, result.Page, result.PageSize, result.TotalCount));
    }
}
