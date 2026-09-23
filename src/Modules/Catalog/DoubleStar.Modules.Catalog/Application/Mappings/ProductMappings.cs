// Application/Mappings/ProductMappings.cs
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.Modules.Catalog.Application.DTOs;
using DoubleStar.Modules.Catalog.Domain.Entities;

namespace DoubleStar.Modules.Catalog.Application.Mappings;

public static class ProductMappings
{
    public static ProductDto ToDto(this Product product) => new(
        product.Id, product.Name, product.Sku, product.Description, product.CategoryId, product.BrandId,
        product.UnitPriceKobo, product.TrackingMode, product.IsArchived);

    public static ProductSummaryDto ToSummaryDto(this Product product) =>
        new(product.Id, product.Name, product.Sku, product.UnitPriceKobo, product.TrackingMode);
}