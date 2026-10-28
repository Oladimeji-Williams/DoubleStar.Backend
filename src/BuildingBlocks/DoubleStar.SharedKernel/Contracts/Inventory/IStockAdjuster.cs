using DoubleStar.SharedKernel.Common.Primitives;

namespace DoubleStar.SharedKernel.Contracts.Inventory;

public interface IStockAdjuster
{
    // Bulk-tracked products
    Task<Result> ReserveAsync(int productId, int quantity, CancellationToken cancellationToken = default);
    Task<Result> DeductAsync(int productId, int quantity, CancellationToken cancellationToken = default);
    Task<Result> DeductSerializedAsync(string serialNumber, CancellationToken cancellationToken = default);
    Task<Result> ReleaseReservationAsync(int productId, int quantity, CancellationToken cancellationToken = default);
    Task<Result> ConsumeReservationAsync(int productId, int quantity, CancellationToken cancellationToken = default);

    // Serialized products (IMEI): reserve "any N in-stock units" and get their serials back.
    Task<Result<IReadOnlyList<string>>> ReserveSerializedAsync(
        int productId, int quantity, CancellationToken cancellationToken = default);

    Task<Result> ReleaseSerializedReservationAsync(
        IReadOnlyCollection<string> serialNumbers, CancellationToken cancellationToken = default);

    Task<Result> ConsumeSerializedReservationAsync(
        IReadOnlyCollection<string> serialNumbers, CancellationToken cancellationToken = default);
}
