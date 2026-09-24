// Application/Queries/GetInventoryValuationQuery/GetInventoryValuationQuery.cs
namespace DoubleStar.Modules.Reporting.Application.Queries.GetInventoryValuationQuery;

public sealed record InventoryValuationDto(long TotalValueKobo, int ProductCount, int LowStockCount);

public sealed record GetInventoryValuationQuery(int LowStockThreshold = 5) : IRequest<Result<InventoryValuationDto>>;