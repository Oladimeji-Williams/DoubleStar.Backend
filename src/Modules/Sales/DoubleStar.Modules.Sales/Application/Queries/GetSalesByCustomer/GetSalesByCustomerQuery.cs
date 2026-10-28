using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Sales.Application.DTOs;

namespace DoubleStar.Modules.Sales.Application.Queries.GetSalesByCustomerQuery;

public sealed record GetSalesByCustomerQuery(
    Guid CustomerId,
    PagedQuery Query)
    : IRequest<Result<PagedResult<SaleDto>>>;