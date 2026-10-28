using DoubleStar.Modules.Reporting.Application.DTOs;
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.SharedKernel.Contracts.Sales;

namespace DoubleStar.Modules.Reporting.Application.Queries.GetTopSellingProductsQuery;

public sealed class GetTopSellingProductsQueryHandler(
    ISalesAnalyticsReader salesAnalyticsReader,
    IProductCatalog productCatalog)
    : IRequestHandler<
        GetTopSellingProductsQuery,
        Result<IReadOnlyList<TopSellingProductReportDto>>>
{
    public async Task<Result<IReadOnlyList<TopSellingProductReportDto>>> Handle(
        GetTopSellingProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await salesAnalyticsReader
            .GetTopSellingProductsAsync(
                request.FromUtc,
                request.ToUtc,
                request.Limit,
                cancellationToken);

        if (products.Count == 0)
        {
            return Result<IReadOnlyList<TopSellingProductReportDto>>
                .Success([]);
        }

        var productIds = products
            .Select(x => x.ProductId)
            .Distinct()
            .ToList();

        var catalogProducts = await productCatalog.GetByIdsAsync(
            productIds,
            cancellationToken);

        var productNames = catalogProducts
            .ToDictionary(
                x => x.Id,
                x => x.Name);

        var result = products
            .Select(product => new TopSellingProductReportDto(
                product.ProductId,
                productNames.TryGetValue(
                    product.ProductId,
                    out var name)
                    ? name
                    : $"Product #{product.ProductId}",
                product.QuantitySold,
                product.RevenueKobo))
            .ToList();

        return Result<IReadOnlyList<TopSellingProductReportDto>>
            .Success(result);
    }
}