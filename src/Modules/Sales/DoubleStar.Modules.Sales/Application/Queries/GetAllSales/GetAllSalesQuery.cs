using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Sales.Application.DTOs;

namespace DoubleStar.Modules.Sales.Application.Queries.GetAllSalesQuery;

public sealed record GetAllSalesQuery(
    PagedQuery Query)
    : IRequest<Result<PagedResult<SaleDto>>>;