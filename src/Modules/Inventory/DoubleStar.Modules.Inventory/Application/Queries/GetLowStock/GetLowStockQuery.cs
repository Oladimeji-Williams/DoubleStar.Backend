// Application/Queries/GetLowStockQuery/GetLowStockQuery.cs
using DoubleStar.SharedKernel.Contracts.Inventory;

namespace DoubleStar.Modules.Inventory.Application.Queries.GetLowStockQuery;

public sealed record GetLowStockQuery(int Threshold) : IRequest<Result<IReadOnlyList<StockLevelDto>>>;