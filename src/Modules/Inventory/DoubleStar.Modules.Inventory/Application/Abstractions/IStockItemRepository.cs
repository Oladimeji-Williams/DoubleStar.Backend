using DoubleStar.Modules.Inventory.Domain.Entities;

namespace DoubleStar.Modules.Inventory.Application.Abstractions;

public interface IStockItemRepository
{
    Task<StockItem?> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockItem>> GetLowStockAsync(int threshold, CancellationToken cancellationToken = default);
    Task AddAsync(StockItem stockItem, CancellationToken cancellationToken = default);
    Task UpdateAsync(StockItem stockItem, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockItem>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockItem>> GetByProductIdsAsync(
        IReadOnlyList<int> productIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// UPDATE ... WHERE on-hand - reserved >= quantity. Returns false if there wasn't enough
    /// (or no row). Replaces read-modify-write, which could oversell under concurrency.
    /// </summary>
    Task<bool> TryReserveAtomicAsync(int productId, int quantity, CancellationToken cancellationToken = default);
}
