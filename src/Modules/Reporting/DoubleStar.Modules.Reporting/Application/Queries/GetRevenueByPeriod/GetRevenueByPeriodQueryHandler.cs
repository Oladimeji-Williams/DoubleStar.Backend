// .../GetRevenueByPeriodQueryHandler.cs
using DoubleStar.SharedKernel.Contracts.Sales;

namespace DoubleStar.Modules.Reporting.Application.Queries.GetRevenueByPeriodQuery;

public sealed class GetRevenueByPeriodQueryHandler(ISalesHistory salesHistory)
    : IRequestHandler<GetRevenueByPeriodQuery, Result<IReadOnlyList<DailyRevenueDto>>>
{
    public async Task<Result<IReadOnlyList<DailyRevenueDto>>> Handle(
        GetRevenueByPeriodQuery request, CancellationToken cancellationToken)
    {
        var sales = await salesHistory.GetAllInRangeAsync(request.FromUtc, request.ToUtc, cancellationToken);

        var byDay = sales
            .GroupBy(s => DateOnly.FromDateTime(s.CompletedAtUtc))
            .OrderBy(g => g.Key)
            .Select(g => new DailyRevenueDto(g.Key, g.Sum(s => s.TotalKobo), g.Count()))
            .ToList();

        return Result<IReadOnlyList<DailyRevenueDto>>.Success(byDay);
    }
}