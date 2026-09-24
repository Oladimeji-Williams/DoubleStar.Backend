// Application/Queries/GetRevenueByPeriodQuery/GetRevenueByPeriodQuery.cs
namespace DoubleStar.Modules.Reporting.Application.Queries.GetRevenueByPeriodQuery;

public sealed record DailyRevenueDto(DateOnly Date, long RevenueKobo, int SalesCount);

public sealed record GetRevenueByPeriodQuery(DateTime FromUtc, DateTime ToUtc)
    : IRequest<Result<IReadOnlyList<DailyRevenueDto>>>;