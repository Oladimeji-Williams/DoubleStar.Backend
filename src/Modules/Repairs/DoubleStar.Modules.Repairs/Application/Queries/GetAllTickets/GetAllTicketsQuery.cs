using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Repairs.Application.DTOs;

namespace DoubleStar.Modules.Repairs.Application.Queries.GetAllTickets;

public sealed record GetAllTicketsQuery(
    PagedQuery Query)
    : IRequest<Result<PagedResult<RepairTicketDto>>>;