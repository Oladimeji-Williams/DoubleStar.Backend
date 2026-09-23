// Application/CrossModule/StockLevelReaderService.cs
using DoubleStar.SharedKernel.Contracts.Inventory;
using DoubleStar.Modules.Inventory.Application.Abstractions;

namespace DoubleStar.Modules.Inventory.Application.CrossModule;

public sealed class StockLevelReaderService(
    IStockItemRepository stockItemRepository, ISerializedUnitRepository serializedUnitRepository)
    : IStockLevelReader
{
    public async Task<StockLevelDto?> GetAvailableAsync(int productId, CancellationToken cancellationToken = default)
    {
        var stockItem = await stockItemRepository.GetByProductIdAsync(productId, cancellationToken);
        return stockItem is null ? null : new StockLevelDto(stockItem.ProductId, stockItem.QuantityAvailable, stockItem.QuantityReserved);
    }

    public async Task<SerializedUnitDto?> GetBySerialAsync(string serialNumber, CancellationToken cancellationToken = default)
    {
        var unit = await serializedUnitRepository.GetBySerialAsync(serialNumber, cancellationToken);
        return unit is null ? null : new SerializedUnitDto(unit.Id, unit.ProductId, unit.SerialNumber, unit.Status.ToString());
    }
}