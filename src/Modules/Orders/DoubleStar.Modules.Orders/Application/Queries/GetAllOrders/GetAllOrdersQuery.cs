// Application/Queries/GetAllOrders/GetAllOrders.cs — staff
using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.DTOs;
using DoubleStar.Modules.Orders.Application.Mappings;

namespace DoubleStar.Modules.Orders.Application.Queries.GetAllOrders;

public sealed record GetAllOrdersQuery(PagedQuery Query) : IRequest<Result<PagedResult<OrderListDto>>>;