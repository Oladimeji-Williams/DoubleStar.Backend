// Catalog/Application/Queries/GetPagedProductsQuery/GetPagedProductsQuery.cs
using DoubleStar.BuildingBlocks.Application.Common;
using DoubleStar.Modules.Catalog.Application.DTOs;

namespace DoubleStar.Modules.Catalog.Application.Queries.GetPagedProductsQuery;

public sealed record GetPagedProductsQuery(int Page, int PageSize) : IRequest<Result<PagedResult<ProductDto>>>;