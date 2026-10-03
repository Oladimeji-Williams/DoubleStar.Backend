// Persistence/Repositories/SerializedUnitRepository.cs
using Microsoft.EntityFrameworkCore;
using DoubleStar.Modules.Inventory.Application.Abstractions;
using DoubleStar.Modules.Inventory.Domain.Entities;
using DoubleStar.Modules.Inventory.Domain.Enums;

namespace DoubleStar.Modules.Inventory.Persistence.Repositories;

public sealed class SerializedUnitRepository(InventoryDbContext dbContext) : ISerializedUnitRepository
{
    public Task<SerializedUnit?> GetBySerialAsync(string serialNumber, CancellationToken cancellationToken = default) =>
        dbContext.SerializedUnits.FirstOrDefaultAsync(
            s => s.SerialNumber == serialNumber.Trim().ToUpperInvariant(), cancellationToken);

    public async Task AddAsync(SerializedUnit unit, CancellationToken cancellationToken = default)
    {
        await dbContext.SerializedUnits.AddAsync(unit, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(SerializedUnit unit, CancellationToken cancellationToken = default)
    {
        dbContext.SerializedUnits.Update(unit);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<int, int>> GetInStockCountsByProductAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.SerializedUnits
            .Where(u => u.Status == SerializedUnitStatus.InStock)
            .GroupBy(u => u.ProductId)
            .Select(g => new { ProductId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProductId, x => x.Count, cancellationToken);
    }
}