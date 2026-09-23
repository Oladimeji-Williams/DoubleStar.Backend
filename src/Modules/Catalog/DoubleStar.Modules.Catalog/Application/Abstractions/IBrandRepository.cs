// Application/Abstractions/IBrandRepository.cs
using DoubleStar.Modules.Catalog.Domain.Entities;

namespace DoubleStar.Modules.Catalog.Application.Abstractions;

public interface IBrandRepository
{
    Task<IReadOnlyList<Brand>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Brand brand, CancellationToken cancellationToken = default);
}