using Microsoft.EntityFrameworkCore;
using DoubleStar.Modules.Inventory.Application.Abstractions;
using DoubleStar.Modules.Inventory.Domain.Entities;

namespace DoubleStar.Modules.Inventory.Persistence.Repositories;

public sealed class StockItemRepository(InventoryDbContext dbContext) : IStockItemRepository
{
    public Task<StockItem?> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default) =>
        dbContext.StockItems.FirstOrDefaultAsync(s => s.ProductId == productId, cancellationToken);

    public async Task<IReadOnlyList<StockItem>> GetLowStockAsync(int threshold, CancellationToken cancellationToken = default) =>
        await dbContext.StockItems
            .Where(s => s.QuantityOnHand - s.QuantityReserved <= threshold)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(StockItem stockItem, CancellationToken cancellationToken = default)
    {
        await dbContext.StockItems.AddAsync(stockItem, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(StockItem stockItem, CancellationToken cancellationToken = default)
    {
        dbContext.StockItems.Update(stockItem);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockItem>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.StockItems.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<StockItem>> GetByProductIdsAsync(
        IReadOnlyList<int> productIds,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.StockItems
            .Where(s => productIds.Contains(s.ProductId))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> TryReserveAtomicAsync(
        int productId, int quantity, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var affected = await dbContext.StockItems
            .Where(s => s.ProductId == productId && s.QuantityOnHand - s.QuantityReserved >= quantity)
            .ExecuteUpdateAsync(
                set => set
                    .SetProperty(s => s.QuantityReserved, s => s.QuantityReserved + quantity)
                    .SetProperty(s => s.ModifiedAt, now),
                cancellationToken);

        return affected == 1;
    }
}
