// .../GetLowStockQueryHandler.cs
using DoubleStar.SharedKernel.Contracts.Inventory;
using DoubleStar.Modules.Inventory.Application.Abstractions;

namespace DoubleStar.Modules.Inventory.Application.Queries.GetLowStockQuery;

public sealed class GetLowStockQueryHandler(IStockItemRepository stockItemRepository)
    : IRequestHandler<GetLowStockQuery, Result<IReadOnlyList<StockLevelDto>>>
{
    public async Task<Result<IReadOnlyList<StockLevelDto>>> Handle(
        GetLowStockQuery request, CancellationToken cancellationToken)
    {
        var items = await stockItemRepository.GetLowStockAsync(request.Threshold, cancellationToken);
        return Result<IReadOnlyList<StockLevelDto>>.Success(
            items.Select(s => new StockLevelDto(s.ProductId, s.QuantityAvailable, s.QuantityReserved)).ToList());
    }
}