using DoubleStar.SharedKernel.Contracts.Inventory;

namespace DoubleStar.Modules.Inventory.Application.Queries.GetStockLevelsQuery;

public sealed record GetStockLevelsQuery(
    IReadOnlyList<int> ProductIds)
    : IRequest<Result<IReadOnlyList<StockLevelDto>>>;