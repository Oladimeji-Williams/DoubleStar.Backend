namespace DoubleStar.SharedKernel.Contracts.Catalog;

public sealed record ProductSummaryDto(
    int Id,
    string Name,
    string Sku,
    long UnitPriceKobo,
    StockTrackingMode TrackingMode,
    bool IsArchived = false,
    string? ImageUrl = null);
