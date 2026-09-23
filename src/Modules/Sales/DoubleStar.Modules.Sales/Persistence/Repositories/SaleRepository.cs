// Persistence/Repositories/SaleRepository.cs
using Microsoft.EntityFrameworkCore;
using DoubleStar.Modules.Sales.Application.Abstractions;
using DoubleStar.Modules.Sales.Domain.Entities;
using DoubleStar.Modules.Sales.Domain.Enums;

namespace DoubleStar.Modules.Sales.Persistence.Repositories;

public sealed class SaleRepository(SalesDbContext dbContext) : ISaleRepository
{
    public Task<Sale?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Sales.Include(s => s.Lines).FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Sale>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Sales.Include(s => s.Lines).OrderByDescending(s => s.CreatedAt).Take(200).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Sale>> GetAllForCustomerAsync(
        Guid customerId, CancellationToken cancellationToken = default) =>
        await dbContext.Sales.Include(s => s.Lines)
            .Where(s => s.CustomerId == customerId && s.Status == SaleStatus.Completed)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Sale>> GetAllInRangeAsync(
        DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default) =>
        await dbContext.Sales.Include(s => s.Lines)
            .Where(s => s.Status == SaleStatus.Completed && s.CreatedAt >= fromUtc && s.CreatedAt <= toUtc)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await dbContext.Sales.AddAsync(sale, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        dbContext.Sales.Update(sale);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}