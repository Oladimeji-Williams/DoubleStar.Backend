namespace DoubleStar.SharedKernel.Contracts.Sales;

public interface ISalesAnalyticsReader
{
    Task<IReadOnlyList<TopSellingProductDto>>
        GetTopSellingProductsAsync(
            DateTime fromUtc,
            DateTime toUtc,
            int limit,
            CancellationToken cancellationToken = default);
}