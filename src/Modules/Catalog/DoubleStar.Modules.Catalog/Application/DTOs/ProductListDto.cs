using DoubleStar.SharedKernel.Contracts.Catalog;

namespace DoubleStar.Modules.Catalog.Application.DTOs;

public sealed record ProductListDto(
    int Id,
    string Name,
    string Sku,
    string? CategoryName,
    string? BrandName,
    long UnitPriceKobo,
    StockTrackingMode TrackingMode,
    bool IsArchived,
    string? ImageUrl);