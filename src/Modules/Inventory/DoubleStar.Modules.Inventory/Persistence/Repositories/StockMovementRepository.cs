// Persistence/Repositories/StockMovementRepository.cs
using DoubleStar.Modules.Inventory.Application.Abstractions;
using DoubleStar.Modules.Inventory.Domain.Entities;

namespace DoubleStar.Modules.Inventory.Persistence.Repositories;

public sealed class StockMovementRepository(InventoryDbContext dbContext) : IStockMovementRepository
{
    public async Task AddAsync(StockMovement movement, CancellationToken cancellationToken = default)
    {
        await dbContext.StockMovements.AddAsync(movement, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}