// .../GetStockLevelQueryHandler.cs
using DoubleStar.SharedKernel.Contracts.Inventory;
using DoubleStar.Modules.Inventory.Application.Abstractions;
using DoubleStar.Modules.Inventory.Application.Errors;

namespace DoubleStar.Modules.Inventory.Application.Queries.GetStockLevelQuery;

public sealed class GetStockLevelQueryHandler(IStockItemRepository stockItemRepository)
    : IRequestHandler<GetStockLevelQuery, Result<StockLevelDto>>
{
    public async Task<Result<StockLevelDto>> Handle(GetStockLevelQuery request, CancellationToken cancellationToken)
    {
        var stockItem = await stockItemRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
        if (stockItem is null)
        {
            return Result<StockLevelDto>.Failure(InventoryErrors.ProductNotTracked(request.ProductId));
        }

        return Result<StockLevelDto>.Success(
            new StockLevelDto(stockItem.ProductId, stockItem.QuantityAvailable, stockItem.QuantityReserved));
    }
}