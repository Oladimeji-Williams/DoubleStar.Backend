// Application/Queries/GetStockLevelQuery/GetStockLevelQuery.cs
using DoubleStar.SharedKernel.Contracts.Inventory;

namespace DoubleStar.Modules.Inventory.Application.Queries.GetStockLevelQuery;

public sealed record GetStockLevelQuery(int ProductId) : IRequest<Result<StockLevelDto>>;