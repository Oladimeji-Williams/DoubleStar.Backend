using DoubleStar.Modules.Reporting.Application.DTOs;

namespace DoubleStar.Modules.Reporting.Application.Queries.GetTopSellingProductsQuery;

public sealed record GetTopSellingProductsQuery(
    DateTime FromUtc,
    DateTime ToUtc,
    int Limit = 10)
    : IRequest<Result<IReadOnlyList<TopSellingProductReportDto>>>;