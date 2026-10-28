using DoubleStar.BuildingBlocks.Application.Common;
using DoubleStar.Modules.Reporting.Application.DTOs;

namespace DoubleStar.Modules.Reporting.Application.Queries.GetPaymentMethodSummaryQuery;

public sealed record GetPaymentMethodSummaryQuery(
    DateTime FromUtc,
    DateTime ToUtc)
    : IRequest<Result<IReadOnlyList<PaymentMethodReportDto>>>;