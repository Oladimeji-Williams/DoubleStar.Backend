using DoubleStar.Modules.Inventory.Domain.Entities;
using DoubleStar.Modules.Inventory.Domain.Enums;

namespace DoubleStar.Modules.Inventory.Application.Abstractions;

public interface ISerializedUnitRepository
{
    Task<SerializedUnit?> GetBySerialAsync(string serialNumber, CancellationToken cancellationToken = default);
    Task AddAsync(SerializedUnit unit, CancellationToken cancellationToken = default);
    Task UpdateAsync(SerializedUnit unit, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<int, int>> GetInStockCountsByProductAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SerializedUnit>> GetInStockByProductAsync(
        int productId, int take, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SerializedUnit>> GetBySerialsAsync(
        IReadOnlyCollection<string> serialNumbers, CancellationToken cancellationToken = default);

    /// <summary>Single conditional UPDATE — two buyers can never win the same unit.</summary>
    Task<bool> TryChangeStatusAsync(
        Guid unitId, SerializedUnitStatus from, SerializedUnitStatus to, CancellationToken cancellationToken = default);
}
