using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.DTOs;
using DoubleStar.Modules.Orders.Application.Mappings;

namespace DoubleStar.Modules.Orders.Application.Queries.GetMyOrders;

public sealed record GetMyOrdersQuery(PagedQuery Query) : IRequest<Result<PagedResult<OrderListDto>>>;