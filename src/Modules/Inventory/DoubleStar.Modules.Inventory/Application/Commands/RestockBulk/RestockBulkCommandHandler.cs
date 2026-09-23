// .../RestockBulkCommandHandler.cs
using DoubleStar.Modules.Inventory.Application.Abstractions;
using DoubleStar.Modules.Inventory.Domain.Entities;
using DoubleStar.Modules.Inventory.Domain.Enums;

namespace DoubleStar.Modules.Inventory.Application.Commands.RestockBulkCommand;

public sealed class RestockBulkCommandHandler(
    IStockItemRepository stockItemRepository, IStockMovementRepository stockMovementRepository)
    : IRequestHandler<RestockBulkCommand, Result>
{
    public async Task<Result> Handle(RestockBulkCommand request, CancellationToken cancellationToken)
    {
        var stockItem = await stockItemRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
        if (stockItem is null)
        {
            stockItem = StockItem.CreateEmpty(request.ProductId);
            await stockItemRepository.AddAsync(stockItem, cancellationToken);
        }

        stockItem.Receive(request.Quantity);
        await stockItemRepository.UpdateAsync(stockItem, cancellationToken);

        await stockMovementRepository.AddAsync(
            StockMovement.Create(request.ProductId, StockMovementType.In, request.Quantity, request.Reference),
            cancellationToken);

        return Result.Success();
    }
}