// Fills ProductDto.AvailableQuantity from the Inventory *contract* (never the Inventory module itself).
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.SharedKernel.Contracts.Inventory;
using DoubleStar.Modules.Catalog.Application.DTOs;

namespace DoubleStar.Modules.Catalog.Application.Mappings;

public static class ProductAvailabilityEnricher
{
    public static async Task<IReadOnlyList<ProductDto>> WithAvailabilityAsync(
        this IReadOnlyList<ProductDto> products, IStockLevelReader stockReader, CancellationToken cancellationToken)
    {
        if (products.Count == 0)
        {
            return products;
        }

        var bulk = (await stockReader.GetAllAsync(cancellationToken)).ToDictionary(s => s.ProductId);
        var serialized = await stockReader.GetSerializedInStockCountsAsync(cancellationToken);

        return products.Select(p => p with { AvailableQuantity = Resolve(p, bulk, serialized) }).ToList();
    }

    public static async Task<ProductDto> WithAvailabilityAsync(
        this ProductDto product, IStockLevelReader stockReader, CancellationToken cancellationToken)
    {
        int available;

        if (product.TrackingMode == StockTrackingMode.Serialized)
        {
            var counts = await stockReader.GetSerializedInStockCountsAsync(cancellationToken);
            available = counts.GetValueOrDefault(product.Id);
        }
        else
        {
            var level = await stockReader.GetAvailableAsync(product.Id, cancellationToken);
            available = Math.Max(0, level?.AvailableQuantity ?? 0);
        }

        return product with { AvailableQuantity = available };
    }

    private static int Resolve(
        ProductDto product,
        IReadOnlyDictionary<int, StockLevelDto> bulk,
        IReadOnlyDictionary<int, int> serialized) =>
        product.TrackingMode == StockTrackingMode.Serialized
            ? serialized.GetValueOrDefault(product.Id)
            : Math.Max(0, bulk.TryGetValue(product.Id, out var level) ? level.AvailableQuantity : 0);
}
