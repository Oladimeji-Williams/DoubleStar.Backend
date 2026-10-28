using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Payments.Application.DTOs;

namespace DoubleStar.Modules.Payments.Application.Queries.GetPagedPayments;

public sealed record GetPagedPaymentsQuery(
    PagedQuery Query)
    : IRequest<Result<PagedResult<PaymentListDto>>>;