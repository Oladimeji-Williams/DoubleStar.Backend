// Domain/Entities/Product.cs
using DoubleStar.SharedKernel.Domain;
using DoubleStar.SharedKernel.Contracts.Catalog;

namespace DoubleStar.Modules.Catalog.Domain.Entities;

public sealed class Product : Entity
{
    public string Name { get; private set; } = null!;
    public string Sku { get; private set; } = null!;
    public string? Description { get; private set; }
    public int? CategoryId { get; private set; }
    public int? BrandId { get; private set; }
    public long UnitPriceKobo { get; private set; }
    public StockTrackingMode TrackingMode { get; private set; }
    public bool IsArchived { get; private set; }

    private Product() { }

    public static Product Create(
        string name, string sku, string? description, int? categoryId, int? brandId,
        long unitPriceKobo, StockTrackingMode trackingMode)
    {
        if (unitPriceKobo < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPriceKobo), "Price cannot be negative.");
        }

        return new Product
        {
            Name = name.Trim(),
            Sku = sku.Trim().ToUpperInvariant(),
            Description = description?.Trim(),
            CategoryId = categoryId,
            BrandId = brandId,
            UnitPriceKobo = unitPriceKobo,
            TrackingMode = trackingMode,
        };
    }

    public void UpdateDetails(string name, string? description, int? categoryId, int? brandId)
    {
        Name = name.Trim();
        Description = description?.Trim();
        CategoryId = categoryId;
        BrandId = brandId;
    }

    public void SetPrice(long unitPriceKobo)
    {
        if (unitPriceKobo < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPriceKobo), "Price cannot be negative.");
        }
        UnitPriceKobo = unitPriceKobo;
    }

    public void Archive() => IsArchived = true;
    public void Unarchive() => IsArchived = false;
}