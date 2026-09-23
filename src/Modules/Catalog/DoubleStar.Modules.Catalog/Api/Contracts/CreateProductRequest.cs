// Api/Contracts/CreateProductRequest.cs
using DoubleStar.SharedKernel.Contracts.Catalog;

namespace DoubleStar.Modules.Catalog.Api.Contracts;

public sealed record CreateProductRequest(
    string Name, string Sku, string? Description, int? CategoryId, int? BrandId,
    long UnitPriceKobo, StockTrackingMode TrackingMode);