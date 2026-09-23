// Application/Abstractions/IStockItemRepository.cs
using DoubleStar.Modules.Inventory.Domain.Entities;

namespace DoubleStar.Modules.Inventory.Application.Abstractions;

public interface IStockItemRepository
{
    Task<StockItem?> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockItem>> GetLowStockAsync(int threshold, CancellationToken cancellationToken = default);
    Task AddAsync(StockItem stockItem, CancellationToken cancellationToken = default);
    Task UpdateAsync(StockItem stockItem, CancellationToken cancellationToken = default);
}