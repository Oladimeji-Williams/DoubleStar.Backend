// Persistence/Repositories/BrandRepository.cs
using Microsoft.EntityFrameworkCore;
using DoubleStar.Modules.Catalog.Application.Abstractions;
using DoubleStar.Modules.Catalog.Domain.Entities;

namespace DoubleStar.Modules.Catalog.Persistence.Repositories;

public sealed class BrandRepository(CatalogDbContext dbContext) : IBrandRepository
{
    public async Task<IReadOnlyList<Brand>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Brands.OrderBy(b => b.Name).ToListAsync(cancellationToken);

    public async Task AddAsync(Brand brand, CancellationToken cancellationToken = default)
    {
        await dbContext.Brands.AddAsync(brand, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}