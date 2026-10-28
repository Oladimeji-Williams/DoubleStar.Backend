using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Catalog.Application.DTOs;

namespace DoubleStar.Modules.Catalog.Application.Queries.GetPagedProducts;

public sealed record GetPagedProductsQuery(
    PagedQuery Query,
    bool IncludeArchived = false)
    : IRequest<Result<PagedResult<ProductDto>>>;