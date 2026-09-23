// Application/DTOs/ProductDto.cs
using DoubleStar.SharedKernel.Contracts.Catalog;

namespace DoubleStar.Modules.Catalog.Application.DTOs;

public sealed record ProductDto(
    int Id, string Name, string Sku, string? Description, int? CategoryId, int? BrandId,
    long UnitPriceKobo, StockTrackingMode TrackingMode, bool IsArchived);