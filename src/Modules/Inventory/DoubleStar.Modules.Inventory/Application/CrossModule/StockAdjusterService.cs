using DoubleStar.SharedKernel.Common.Primitives;
using DoubleStar.SharedKernel.Contracts.Inventory;
using DoubleStar.Modules.Inventory.Application.Abstractions;
using DoubleStar.Modules.Inventory.Application.Errors;
using DoubleStar.Modules.Inventory.Domain.Entities;
using DoubleStar.Modules.Inventory.Domain.Enums;

namespace DoubleStar.Modules.Inventory.Application.CrossModule;

/// <summary>
/// Called directly by Sales, Repairs and Orders (via the IStockAdjuster contract) at the
/// point they need to touch stock — deliberately synchronous rather than event-driven,
/// since overselling is a correctness bug, not something eventual consistency should paper over.
/// </summary>
public sealed class StockAdjusterService(
    IStockItemRepository stockItemRepository,
    ISerializedUnitRepository serializedUnitRepository,
    IStockMovementRepository stockMovementRepository)
    : IStockAdjuster
{
    private const int MaxSerializedReserveAttempts = 3;

    private static Error InvalidQuantity() => new(
        "Inventory.InvalidQuantity", "Quantity must be greater than zero.", ErrorType.Validation);

    public async Task<Result> ReserveAsync(int productId, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            return Result.Failure(InvalidQuantity());
        }

        if (await stockItemRepository.TryReserveAtomicAsync(productId, quantity, cancellationToken))
        {
            await stockMovementRepository.AddAsync(
                StockMovement.Create(productId, StockMovementType.Reservation, quantity, reference: null),
                cancellationToken);

            return Result.Success();
        }

        // Atomic update touched nothing: either no stock row at all, or not enough available.
        var stockItem = await stockItemRepository.GetByProductIdAsync(productId, cancellationToken);

        return stockItem is null
            ? Result.Failure(InventoryErrors.ProductNotTracked(productId))
            : Result.Failure(InventoryErrors.InsufficientStock(productId));
    }

    public async Task<Result> ReleaseReservationAsync(int productId, int quantity, CancellationToken cancellationToken = default)
    {
        var stockItem = await stockItemRepository.GetByProductIdAsync(productId, cancellationToken);
        if (stockItem is null)
        {
            return Result.Failure(InventoryErrors.ProductNotTracked(productId));
        }

        stockItem.ReleaseReservation(quantity);
        await stockItemRepository.UpdateAsync(stockItem, cancellationToken);
        await stockMovementRepository.AddAsync(
            StockMovement.Create(productId, StockMovementType.ReservationRelease, quantity, reference: null), cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeductAsync(int productId, int quantity, CancellationToken cancellationToken = default)
    {
        var stockItem = await stockItemRepository.GetByProductIdAsync(productId, cancellationToken);
        if (stockItem is null)
        {
            return Result.Failure(InventoryErrors.ProductNotTracked(productId));
        }

        if (!stockItem.TryDeduct(quantity))
        {
            return Result.Failure(InventoryErrors.InsufficientStock(productId));
        }

        await stockItemRepository.UpdateAsync(stockItem, cancellationToken);
        await stockMovementRepository.AddAsync(
            StockMovement.Create(productId, StockMovementType.Out, quantity, reference: null), cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeductSerializedAsync(string serialNumber, CancellationToken cancellationToken = default)
    {
        var unit = await serializedUnitRepository.GetBySerialAsync(serialNumber, cancellationToken);
        if (unit is null)
        {
            return Result.Failure(InventoryErrors.SerialNotFound(serialNumber));
        }

        if (!unit.TryMarkSold())
        {
            return Result.Failure(InventoryErrors.SerialAlreadySold(serialNumber));
        }

        await serializedUnitRepository.UpdateAsync(unit, cancellationToken);
        await stockMovementRepository.AddAsync(
            StockMovement.Create(unit.ProductId, StockMovementType.Out, 1, serialNumber), cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ConsumeReservationAsync(
        int productId, int quantity, CancellationToken cancellationToken = default)
    {
        var stockItem = await stockItemRepository.GetByProductIdAsync(productId, cancellationToken);
        if (stockItem is null)
        {
            return Result.Failure(InventoryErrors.ProductNotTracked(productId));
        }

        if (!stockItem.TryConsumeReservation(quantity))
        {
            return Result.Failure(InventoryErrors.InsufficientStock(productId));
        }

        await stockItemRepository.UpdateAsync(stockItem, cancellationToken);
        await stockMovementRepository.AddAsync(
            StockMovement.Create(productId, StockMovementType.Out, quantity, reference: null), cancellationToken);

        return Result.Success();
    }

    // ---------------------------------------------------------------- serialized (IMEI) products

    public async Task<Result<IReadOnlyList<string>>> ReserveSerializedAsync(
        int productId, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            return Result<IReadOnlyList<string>>.Failure(InvalidQuantity());
        }

        var reserved = new List<SerializedUnit>();

        for (var attempt = 0; attempt < MaxSerializedReserveAttempts && reserved.Count < quantity; attempt++)
        {
            var needed = quantity - reserved.Count;

            // Over-fetch: another buyer may win some candidates between our read and our UPDATE.
            var candidates = await serializedUnitRepository.GetInStockByProductAsync(
                productId, (needed * 2) + 5, cancellationToken);

            if (candidates.Count == 0)
            {
                break;
            }

            foreach (var unit in candidates)
            {
                if (reserved.Count == quantity)
                {
                    break;
                }

                if (await serializedUnitRepository.TryChangeStatusAsync(
                        unit.Id, SerializedUnitStatus.InStock, SerializedUnitStatus.Reserved, cancellationToken))
                {
                    reserved.Add(unit);
                }
            }
        }

        if (reserved.Count < quantity)
        {
            foreach (var unit in reserved)
            {
                await serializedUnitRepository.TryChangeStatusAsync(
                    unit.Id, SerializedUnitStatus.Reserved, SerializedUnitStatus.InStock, cancellationToken);
            }

            return Result<IReadOnlyList<string>>.Failure(InventoryErrors.InsufficientStock(productId));
        }

        await stockMovementRepository.AddAsync(
            StockMovement.Create(productId, StockMovementType.Reservation, quantity, reference: null),
            cancellationToken);

        return Result<IReadOnlyList<string>>.Success(reserved.Select(u => u.SerialNumber).ToList());
    }

    public async Task<Result> ReleaseSerializedReservationAsync(
        IReadOnlyCollection<string> serialNumbers, CancellationToken cancellationToken = default)
    {
        if (serialNumbers.Count == 0)
        {
            return Result.Success();
        }

        var units = await serializedUnitRepository.GetBySerialsAsync(serialNumbers, cancellationToken);

        foreach (var unit in units.Where(u => u.Status == SerializedUnitStatus.Reserved))
        {
            if (await serializedUnitRepository.TryChangeStatusAsync(
                    unit.Id, SerializedUnitStatus.Reserved, SerializedUnitStatus.InStock, cancellationToken))
            {
                await stockMovementRepository.AddAsync(
                    StockMovement.Create(unit.ProductId, StockMovementType.ReservationRelease, 1, unit.SerialNumber),
                    cancellationToken);
            }
        }

        // Units that are InStock/Sold already are left alone: releasing twice must be harmless.
        return Result.Success();
    }

    public async Task<Result> ConsumeSerializedReservationAsync(
        IReadOnlyCollection<string> serialNumbers, CancellationToken cancellationToken = default)
    {
        if (serialNumbers.Count == 0)
        {
            return Result.Success();
        }

        var units = await serializedUnitRepository.GetBySerialsAsync(serialNumbers, cancellationToken);

        if (units.Count != serialNumbers.Count)
        {
            return Result.Failure(InventoryErrors.SerialNotFound(string.Join(", ", serialNumbers)));
        }

        foreach (var unit in units)
        {
            if (unit.Status == SerializedUnitStatus.Sold)
            {
                continue; // already consumed by an earlier delivery of the same event
            }

            if (!await serializedUnitRepository.TryChangeStatusAsync(
                    unit.Id, SerializedUnitStatus.Reserved, SerializedUnitStatus.Sold, cancellationToken))
            {
                return Result.Failure(InventoryErrors.SerialAlreadySold(unit.SerialNumber));
            }

            await stockMovementRepository.AddAsync(
                StockMovement.Create(unit.ProductId, StockMovementType.Out, 1, unit.SerialNumber),
                cancellationToken);
        }

        return Result.Success();
    }
}
