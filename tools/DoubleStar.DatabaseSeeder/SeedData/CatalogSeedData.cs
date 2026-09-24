// SeedData/CatalogSeedData.cs
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.Modules.Catalog.Domain.Entities;
using DoubleStar.Modules.Catalog.Persistence;

namespace DoubleStar.DatabaseSeeder.SeedData;

public sealed record CatalogSeedResult(IReadOnlyList<Product> BulkProducts, IReadOnlyList<Product> SerializedProducts);

public static class CatalogSeedData
{
    public static async Task<CatalogSeedResult> SeedAsync(CatalogDbContext dbContext)
    {
        var phones = Category.Create("Phones");
        var accessories = Category.Create("Accessories");
        var repairParts = Category.Create("Repair Parts");
        await dbContext.Categories.AddRangeAsync(phones, accessories, repairParts);

        var apple = Brand.Create("Apple");
        var samsung = Brand.Create("Samsung");
        var tecno = Brand.Create("Tecno");
        var generic = Brand.Create("Generic");
        await dbContext.Brands.AddRangeAsync(apple, samsung, tecno, generic);

        await dbContext.SaveChangesAsync(); // need generated Ids before referencing them below

        var serializedProducts = new[]
        {
            Product.Create("iPhone 13, 128GB", "IP13-128", "Refurbished, grade A", phones.Id, apple.Id, 450_000_00, StockTrackingMode.Serialized),
            Product.Create("Samsung Galaxy A54", "SGA54", "Brand new, sealed", phones.Id, samsung.Id, 280_000_00, StockTrackingMode.Serialized),
            Product.Create("Tecno Spark 20", "TS20", "Brand new, sealed", phones.Id, tecno.Id, 125_000_00, StockTrackingMode.Serialized),
        };

        var bulkProducts = new[]
        {
            Product.Create("USB-C Charging Cable", "CBL-USBC", "1 metre, fast charge", accessories.Id, generic.Id, 1_500_00, StockTrackingMode.Bulk),
            Product.Create("Tempered Glass Screen Protector", "SP-TG", "Fits most 6.1\" phones", accessories.Id, generic.Id, 2_000_00, StockTrackingMode.Bulk),
            Product.Create("iPhone Replacement Battery", "BAT-IP", "OEM-equivalent", repairParts.Id, apple.Id, 8_000_00, StockTrackingMode.Bulk),
            Product.Create("Samsung Replacement Screen", "SCR-SAM", "OEM-equivalent", repairParts.Id, samsung.Id, 22_000_00, StockTrackingMode.Bulk),
        };

        await dbContext.Products.AddRangeAsync(serializedProducts);
        await dbContext.Products.AddRangeAsync(bulkProducts);
        await dbContext.SaveChangesAsync();

        return new CatalogSeedResult(bulkProducts, serializedProducts);
    }
}