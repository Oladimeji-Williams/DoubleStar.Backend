// Application/CrossModule/StockAdjusterService.cs
using DoubleStar.SharedKernel.Common.Primitives;
using DoubleStar.SharedKernel.Contracts.Inventory;
using DoubleStar.Modules.Inventory.Application.Abstractions;
using DoubleStar.Modules.Inventory.Application.Errors;
using DoubleStar.Modules.Inventory.Domain.Entities;
using DoubleStar.Modules.Inventory.Domain.Enums;

namespace DoubleStar.Modules.Inventory.Application.CrossModule;

/// <summary>
/// Called directly by Sales and Repairs (via the IStockAdjuster contract) at
/// the point they need to touch stock — deliberately synchronous rather than
/// event-driven, since overselling is a correctness bug, not something
/// eventual consistency should be allowed to paper over.
/// </summary>
public sealed class StockAdjusterService(
    IStockItemRepository stockItemRepository,
    ISerializedUnitRepository serializedUnitRepository,
    IStockMovementRepository stockMovementRepository)
    : IStockAdjuster
{
    public async Task<Result> ReserveAsync(int productId, int quantity, CancellationToken cancellationToken = default)
    {
        var stockItem = await stockItemRepository.GetByProductIdAsync(productId, cancellationToken);
        if (stockItem is null)
        {
            return Result.Failure(InventoryErrors.ProductNotTracked(productId));
        }

        if (!stockItem.TryReserve(quantity))
        {
            return Result.Failure(InventoryErrors.InsufficientStock(productId));
        }

        await stockItemRepository.UpdateAsync(stockItem, cancellationToken);
        await stockMovementRepository.AddAsync(
            StockMovement.Create(productId, StockMovementType.Reservation, quantity, reference: null), cancellationToken);

        return Result.Success();
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
}