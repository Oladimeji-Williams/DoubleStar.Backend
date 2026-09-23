// Contracts/Inventory/IStockAdjuster.cs
using DoubleStar.SharedKernel.Common.Primitives;

namespace DoubleStar.SharedKernel.Contracts.Inventory;

public interface IStockAdjuster
{
    Task<Result> ReserveAsync(int productId, int quantity, CancellationToken cancellationToken = default);
    Task<Result> DeductAsync(int productId, int quantity, CancellationToken cancellationToken = default);
    Task<Result> DeductSerializedAsync(string serialNumber, CancellationToken cancellationToken = default);
    Task<Result> ReleaseReservationAsync(int productId, int quantity, CancellationToken cancellationToken = default);
}