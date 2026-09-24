// .../GetInventoryValuationQueryHandler.cs
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.SharedKernel.Contracts.Inventory;

namespace DoubleStar.Modules.Reporting.Application.Queries.GetInventoryValuationQuery;

public sealed class GetInventoryValuationQueryHandler(IProductCatalog productCatalog, IStockLevelReader stockLevelReader)
    : IRequestHandler<GetInventoryValuationQuery, Result<InventoryValuationDto>>
{
    public async Task<Result<InventoryValuationDto>> Handle(
        GetInventoryValuationQuery request, CancellationToken cancellationToken)
    {
        var products = await productCatalog.GetAllAsync(cancellationToken);
        var stockLevels = await stockLevelReader.GetAllAsync(cancellationToken);
        var stockByProductId = stockLevels.ToDictionary(s => s.ProductId);

        long totalValue = 0;
        var lowStockCount = 0;

        foreach (var product in products)
        {
            if (!stockByProductId.TryGetValue(product.Id, out var stock))
            {
                continue; // no stock record yet — a serialized-only product with nothing received, or Bulk never restocked
            }

            totalValue += stock.AvailableQuantity * product.UnitPriceKobo;

            if (stock.AvailableQuantity <= request.LowStockThreshold)
            {
                lowStockCount++;
            }
        }

        return Result<InventoryValuationDto>.Success(new InventoryValuationDto(totalValue, products.Count, lowStockCount));
    }
}