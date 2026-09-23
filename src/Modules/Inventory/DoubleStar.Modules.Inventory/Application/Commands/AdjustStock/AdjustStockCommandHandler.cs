// .../AdjustStockCommandHandler.cs
using DoubleStar.Modules.Inventory.Application.Abstractions;
using DoubleStar.Modules.Inventory.Domain.Entities;
using DoubleStar.Modules.Inventory.Domain.Enums;

namespace DoubleStar.Modules.Inventory.Application.Commands.AdjustStockCommand;

public sealed class AdjustStockCommandHandler(
    IStockItemRepository stockItemRepository, IStockMovementRepository stockMovementRepository)
    : IRequestHandler<AdjustStockCommand, Result>
{
    public async Task<Result> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        var stockItem = await stockItemRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
        if (stockItem is null)
        {
            stockItem = StockItem.CreateEmpty(request.ProductId);
            await stockItemRepository.AddAsync(stockItem, cancellationToken);
        }

        var delta = request.NewQuantity - stockItem.QuantityOnHand;
        stockItem.SetOnHand(request.NewQuantity);
        await stockItemRepository.UpdateAsync(stockItem, cancellationToken);

        await stockMovementRepository.AddAsync(
            StockMovement.Create(request.ProductId, StockMovementType.Adjustment, delta, request.Reason),
            cancellationToken);

        return Result.Success();
    }
}