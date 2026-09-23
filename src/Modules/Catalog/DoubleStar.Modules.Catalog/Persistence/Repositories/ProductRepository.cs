// Persistence/Repositories/ProductRepository.cs
using Microsoft.EntityFrameworkCore;
using DoubleStar.Modules.Catalog.Application.Abstractions;
using DoubleStar.Modules.Catalog.Domain.Entities;

namespace DoubleStar.Modules.Catalog.Persistence.Repositories;

public sealed class ProductRepository(CatalogDbContext dbContext) : IProductRepository
{
    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(
        IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToList();
        return await dbContext.Products.Where(p => idList.Contains(p.Id)).ToListAsync(cancellationToken);
    }

    public Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default) =>
        dbContext.Products.FirstOrDefaultAsync(p => p.Sku == sku.ToUpperInvariant(), cancellationToken);

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Products.Where(p => !p.IsArchived).OrderBy(p => p.Name).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Product>> SearchAsync(string term, CancellationToken cancellationToken = default)
    {
        var pattern = $"%{term}%";
        return await dbContext.Products
            .Where(p => !p.IsArchived && (EF.Functions.ILike(p.Name, pattern) || EF.Functions.ILike(p.Sku, pattern)))
            .OrderBy(p => p.Name)
            .Take(50)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await dbContext.Products.AddAsync(product, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        dbContext.Products.Update(product);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}