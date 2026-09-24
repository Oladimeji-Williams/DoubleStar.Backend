// SharedKernel/Contracts/Inventory/IStockLevelReader.cs — replace the whole file
namespace DoubleStar.SharedKernel.Contracts.Inventory;

public interface IStockLevelReader
{
    Task<StockLevelDto?> GetAvailableAsync(int productId, CancellationToken cancellationToken = default);
    Task<SerializedUnitDto?> GetBySerialAsync(string serialNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockLevelDto>> GetAllAsync(CancellationToken cancellationToken = default);
}