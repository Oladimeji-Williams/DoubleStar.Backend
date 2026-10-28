using DoubleStar.Modules.Inventory.Application.Abstractions;
using DoubleStar.SharedKernel.Contracts.Inventory;

namespace DoubleStar.Modules.Inventory.Application.Queries.GetStockLevelsQuery;

public sealed class GetStockLevelsQueryHandler(
    IStockItemRepository stockItemRepository)
    : IRequestHandler<
        GetStockLevelsQuery,
        Result<IReadOnlyList<StockLevelDto>>>
{
    public async Task<Result<IReadOnlyList<StockLevelDto>>> Handle(
        GetStockLevelsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ProductIds.Count == 0)
        {
            return Result<IReadOnlyList<StockLevelDto>>.Success(
                Array.Empty<StockLevelDto>());
        }

        var items =
            await stockItemRepository.GetByProductIdsAsync(
                request.ProductIds,
                cancellationToken);

        var result = items
            .Select(stock => new StockLevelDto(
                stock.ProductId,
                stock.QuantityAvailable,
                stock.QuantityReserved))
            .ToList();

        return Result<IReadOnlyList<StockLevelDto>>.Success(result);
    }
}