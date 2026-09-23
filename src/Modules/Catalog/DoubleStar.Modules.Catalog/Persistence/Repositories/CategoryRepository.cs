// Persistence/Repositories/CategoryRepository.cs
using Microsoft.EntityFrameworkCore;
using DoubleStar.Modules.Catalog.Application.Abstractions;
using DoubleStar.Modules.Catalog.Domain.Entities;

namespace DoubleStar.Modules.Catalog.Persistence.Repositories;

public sealed class CategoryRepository(CatalogDbContext dbContext) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Categories.OrderBy(c => c.Name).ToListAsync(cancellationToken);

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await dbContext.Categories.AddAsync(category, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}