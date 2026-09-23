// Contracts/Catalog/ProductSummaryDto.cs
namespace DoubleStar.SharedKernel.Contracts.Catalog;

public sealed record ProductSummaryDto(int Id, string Name, string Sku, long UnitPriceKobo, StockTrackingMode TrackingMode);