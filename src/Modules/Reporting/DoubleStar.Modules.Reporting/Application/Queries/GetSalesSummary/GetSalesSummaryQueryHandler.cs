// .../GetSalesSummaryQueryHandler.cs
using DoubleStar.SharedKernel.Contracts.Sales;

namespace DoubleStar.Modules.Reporting.Application.Queries.GetSalesSummaryQuery;

public sealed class GetSalesSummaryQueryHandler(ISalesHistory salesHistory)
    : IRequestHandler<GetSalesSummaryQuery, Result<SalesSummaryDto>>
{
    public async Task<Result<SalesSummaryDto>> Handle(GetSalesSummaryQuery request, CancellationToken cancellationToken)
    {
        var sales = await salesHistory.GetAllInRangeAsync(request.FromUtc, request.ToUtc, cancellationToken);

        var totalRevenue = sales.Sum(s => s.TotalKobo);
        var count = sales.Count;
        var average = count == 0 ? 0 : totalRevenue / count;

        return Result<SalesSummaryDto>.Success(new SalesSummaryDto(totalRevenue, count, average));
    }
}