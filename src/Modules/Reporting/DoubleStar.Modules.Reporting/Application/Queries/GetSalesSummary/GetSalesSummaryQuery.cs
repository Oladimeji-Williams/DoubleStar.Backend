// Application/Queries/GetSalesSummaryQuery/GetSalesSummaryQuery.cs
namespace DoubleStar.Modules.Reporting.Application.Queries.GetSalesSummaryQuery;

public sealed record SalesSummaryDto(long TotalRevenueKobo, int SalesCount, long AverageSaleKobo);

public sealed record GetSalesSummaryQuery(DateTime FromUtc, DateTime ToUtc) : IRequest<Result<SalesSummaryDto>>;