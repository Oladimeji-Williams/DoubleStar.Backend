// Reporting/Application/Queries/GetInventoryValuationQuery/GetInventoryValuationQueryHandler.cs — full replacement
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.SharedKernel.Contracts.Inventory;

namespace DoubleStar.Modules.Reporting.Application.Queries.GetInventoryValuationQuery;

public sealed class GetInventoryValuationQueryHandler(IProductCatalog productCatalog, IStockLevelReader stockLevelReader)
    : IRequestHandler<GetInventoryValuationQuery, Result<InventoryValuationDto>>
{
    public async Task<Result<InventoryValuationDto>> Handle(GetInventoryValuationQuery request, CancellationToken cancellationToken)
    {
        var products = await productCatalog.GetAllAsync(cancellationToken);
        var stockLevels = await stockLevelReader.GetAllAsync(cancellationToken);
        var serializedCounts = await stockLevelReader.GetSerializedInStockCountsAsync(cancellationToken);
        var stockByProductId = stockLevels.ToDictionary(s => s.ProductId);

        long totalValue = 0;
        var lowStockCount = 0;

        foreach (var product in products)
        {
            if (product.TrackingMode == StockTrackingMode.Serialized)
            {
                var unitsInStock = serializedCounts.GetValueOrDefault(product.Id, 0);
                totalValue += unitsInStock * product.UnitPriceKobo;
                if (unitsInStock <= request.LowStockThreshold) lowStockCount++;
                continue;
            }

            if (!stockByProductId.TryGetValue(product.Id, out var stock)) continue;

            totalValue += stock.AvailableQuantity * product.UnitPriceKobo;
            if (stock.AvailableQuantity <= request.LowStockThreshold) lowStockCount++;
        }

        return Result<InventoryValuationDto>.Success(new InventoryValuationDto(totalValue, products.Count, lowStockCount));
    }
}