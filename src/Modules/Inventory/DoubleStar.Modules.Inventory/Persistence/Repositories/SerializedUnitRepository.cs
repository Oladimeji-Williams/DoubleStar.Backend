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

    public async Task<IReadOnlyList<SerializedUnit>> GetInStockByProductAsync(
        int productId, int take, CancellationToken cancellationToken = default) =>
        await dbContext.SerializedUnits
            .AsNoTracking()
            .Where(u => u.ProductId == productId && u.Status == SerializedUnitStatus.InStock)
            .OrderBy(u => u.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SerializedUnit>> GetBySerialsAsync(
        IReadOnlyCollection<string> serialNumbers, CancellationToken cancellationToken = default)
    {
        var normalized = serialNumbers.Select(s => s.Trim().ToUpperInvariant()).ToList();

        return await dbContext.SerializedUnits
            .AsNoTracking()
            .Where(u => normalized.Contains(u.SerialNumber))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> TryChangeStatusAsync(
        Guid unitId, SerializedUnitStatus from, SerializedUnitStatus to, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var affected = await dbContext.SerializedUnits
            .Where(u => u.Id == unitId && u.Status == from)
            .ExecuteUpdateAsync(
                set => set
                    .SetProperty(u => u.Status, to)
                    .SetProperty(u => u.ModifiedAt, now),
                cancellationToken);

        return affected == 1;
    }
}
