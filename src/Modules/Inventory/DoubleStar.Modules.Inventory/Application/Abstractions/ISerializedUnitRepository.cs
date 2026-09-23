// Application/Abstractions/ISerializedUnitRepository.cs
using DoubleStar.Modules.Inventory.Domain.Entities;

namespace DoubleStar.Modules.Inventory.Application.Abstractions;

public interface ISerializedUnitRepository
{
    Task<SerializedUnit?> GetBySerialAsync(string serialNumber, CancellationToken cancellationToken = default);
    Task AddAsync(SerializedUnit unit, CancellationToken cancellationToken = default);
    Task UpdateAsync(SerializedUnit unit, CancellationToken cancellationToken = default);
}