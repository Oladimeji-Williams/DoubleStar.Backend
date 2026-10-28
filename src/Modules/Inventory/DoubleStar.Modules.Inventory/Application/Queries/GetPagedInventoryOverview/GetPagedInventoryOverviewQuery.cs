using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Inventory.Application.DTOs;
using MediatR;

namespace DoubleStar.Modules.Inventory.Application.Queries.GetPagedInventoryOverview;

public sealed record GetPagedInventoryOverviewQuery(
    PagedQuery Query)
    : IRequest<Result<PagedResult<InventoryOverviewDto>>>;