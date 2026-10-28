using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Repairs.Application.DTOs;

namespace DoubleStar.Modules.Repairs.Application.Queries.GetTicketsByCustomer;

public sealed record GetTicketsByCustomerQuery(
    Guid CustomerId,
    PagedQuery Query)
    : IRequest<Result<PagedResult<RepairTicketDto>>>;