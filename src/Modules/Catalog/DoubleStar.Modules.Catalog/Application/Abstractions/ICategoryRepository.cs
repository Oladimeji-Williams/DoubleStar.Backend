// Application/Abstractions/ICategoryRepository.cs
using DoubleStar.Modules.Catalog.Domain.Entities;

namespace DoubleStar.Modules.Catalog.Application.Abstractions;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Category category, CancellationToken cancellationToken = default);
}