using DoubleStar.Modules.Sales.Domain.Enums;
using DoubleStar.SharedKernel.Contracts.Sales;
using Microsoft.EntityFrameworkCore;

namespace DoubleStar.Modules.Sales.Persistence.Analytics;

public sealed class SalesAnalyticsReader(
    SalesDbContext dbContext)
    : ISalesAnalyticsReader
{
    public async Task<IReadOnlyList<TopSellingProductDto>>
        GetTopSellingProductsAsync(
            DateTime fromUtc,
            DateTime toUtc,
            int limit,
            CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 100);

        var results =
            await (
                from line in dbContext.SaleLines.AsNoTracking()
                join sale in dbContext.Sales.AsNoTracking()
                    on line.SaleId equals sale.Id
                where
                    sale.Status == SaleStatus.Completed &&
                    sale.CompletedAtUtc >= fromUtc &&
                    sale.CompletedAtUtc <= toUtc
                select new
                {
                    line.ProductId,
                    line.Quantity,
                    line.UnitPriceKobo
                }
            )
            .GroupBy(x => x.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                QuantitySold = group.Sum(x => x.Quantity),
                RevenueKobo = group.Sum(
                    x => x.UnitPriceKobo * x.Quantity)
            })
            .OrderByDescending(x => x.QuantitySold)
            .ThenByDescending(x => x.RevenueKobo)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return results
            .Select(x =>
                new TopSellingProductDto(
                    x.ProductId,
                    x.QuantitySold,
                    x.RevenueKobo))
            .ToList();
    }
}