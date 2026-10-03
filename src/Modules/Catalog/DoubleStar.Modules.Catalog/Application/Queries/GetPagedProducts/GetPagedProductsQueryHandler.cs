// .../GetPagedProductsQueryHandler.cs
using DoubleStar.BuildingBlocks.Application.Common;
using DoubleStar.Modules.Catalog.Application.Abstractions;
using DoubleStar.Modules.Catalog.Application.DTOs;
using DoubleStar.Modules.Catalog.Application.Mappings;

namespace DoubleStar.Modules.Catalog.Application.Queries.GetPagedProductsQuery;

public sealed class GetPagedProductsQueryHandler(IProductRepository productRepository)
    : IRequestHandler<GetPagedProductsQuery, Result<PagedResult<ProductDto>>>
{
    public async Task<Result<PagedResult<ProductDto>>> Handle(GetPagedProductsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await productRepository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
        return Result<PagedResult<ProductDto>>.Success(
            new PagedResult<ProductDto>(items.Select(p => p.ToDto()).ToList(), request.Page, request.PageSize, totalCount));
    }
}