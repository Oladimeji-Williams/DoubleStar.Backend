// SeedData/InventorySeedData.cs
using DoubleStar.Modules.Inventory.Domain.Entities;
using DoubleStar.Modules.Inventory.Persistence;

namespace DoubleStar.DatabaseSeeder.SeedData;

public static class InventorySeedData
{
    public static async Task SeedAsync(InventoryDbContext dbContext, CatalogSeedResult catalog)
    {
        foreach (var product in catalog.BulkProducts)
        {
            var stockItem = StockItem.CreateEmpty(product.Id);
            stockItem.Receive(50);
            await dbContext.StockItems.AddAsync(stockItem);
        }

        var serialCounter = 1;
        foreach (var product in catalog.SerializedProducts)
        {
            for (var i = 0; i < 3; i++)
            {
                await dbContext.SerializedUnits.AddAsync(SerializedUnit.Receive(product.Id, $"DSIMEI{serialCounter:D8}"));
                serialCounter++;
            }
        }

        await dbContext.SaveChangesAsync();
    }
}