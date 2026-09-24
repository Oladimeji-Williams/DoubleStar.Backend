// SeedData/SampleSaleSeedData.cs
using Microsoft.EntityFrameworkCore;
using DoubleStar.Modules.Sales.Domain.Entities;
using DoubleStar.Modules.Sales.Persistence;
using DoubleStar.Modules.Inventory.Persistence;
using DoubleStar.Modules.Customers.Domain.Entities;

namespace DoubleStar.DatabaseSeeder.SeedData;

public static class SampleSaleSeedData
{
    public static async Task SeedAsync(
        SalesDbContext salesDb, InventoryDbContext inventoryDb, IReadOnlyList<Customer> customers, CatalogSeedResult catalog)
    {
        var product = catalog.BulkProducts[0]; // USB-C Charging Cable
        const int quantity = 2;

        var sale = Sale.CreateDraft(customers[0].Id);
        var line = sale.AddLine(product.Id, product.UnitPriceKobo, quantity, serialNumber: null);

        var stockItem = await inventoryDb.StockItems.FirstAsync(s => s.ProductId == product.Id);
        stockItem.TryDeduct(quantity);
        sale.MarkLineDeducted(line.Id);
        sale.Complete();

        await salesDb.Sales.AddAsync(sale);
        await salesDb.SaveChangesAsync();
        await inventoryDb.SaveChangesAsync();
    }
}