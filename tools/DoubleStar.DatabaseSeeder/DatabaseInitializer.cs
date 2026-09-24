// DatabaseInitializer.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DoubleStar.Modules.Identity.Infrastructure.Identity;
using DoubleStar.Modules.Identity.Persistence;
using DoubleStar.Modules.Customers.Persistence;
using DoubleStar.Modules.Catalog.Persistence;
using DoubleStar.Modules.Inventory.Persistence;
using DoubleStar.Modules.Sales.Persistence;
using DoubleStar.Modules.Repairs.Persistence;
using DoubleStar.Modules.Payments.Persistence;
using DoubleStar.Modules.Notifications.Persistence;
using DoubleStar.DatabaseSeeder.SeedData;

namespace DoubleStar.DatabaseSeeder;

/// <summary>Destructive dev-reset tool — drops and recreates every schema. Never point this at a real database.</summary>
public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var identityDb = sp.GetRequiredService<ApplicationIdentityDbContext>();
        var customersDb = sp.GetRequiredService<CustomersDbContext>();
        var catalogDb = sp.GetRequiredService<CatalogDbContext>();
        var inventoryDb = sp.GetRequiredService<InventoryDbContext>();
        var salesDb = sp.GetRequiredService<SalesDbContext>();
        var repairsDb = sp.GetRequiredService<RepairsDbContext>();
        var paymentsDb = sp.GetRequiredService<PaymentsDbContext>();
        var notificationsDb = sp.GetRequiredService<NotificationsDbContext>();

        Console.WriteLine("Deleting databases...");
        await identityDb.Database.EnsureDeletedAsync();
        await customersDb.Database.EnsureDeletedAsync();
        await catalogDb.Database.EnsureDeletedAsync();
        await inventoryDb.Database.EnsureDeletedAsync();
        await salesDb.Database.EnsureDeletedAsync();
        await repairsDb.Database.EnsureDeletedAsync();
        await paymentsDb.Database.EnsureDeletedAsync();
        await notificationsDb.Database.EnsureDeletedAsync();

        Console.WriteLine("Applying migrations...");
        await identityDb.Database.MigrateAsync();
        await customersDb.Database.MigrateAsync();
        await catalogDb.Database.MigrateAsync();
        await inventoryDb.Database.MigrateAsync();
        await salesDb.Database.MigrateAsync();
        await repairsDb.Database.MigrateAsync();
        await paymentsDb.Database.MigrateAsync();
        await notificationsDb.Database.MigrateAsync();

        Console.WriteLine("Seeding roles and bootstrap admin...");
        await IdentitySeeder.SeedAsync(sp);

        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

        Console.WriteLine("Seeding staff...");
        var (_, _, technician) = await StaffSeedData.SeedAsync(userManager);

        Console.WriteLine("Seeding customers...");
        var customers = await CustomerSeedData.SeedAsync(customersDb);

        Console.WriteLine("Seeding catalog...");
        var catalog = await CatalogSeedData.SeedAsync(catalogDb);

        Console.WriteLine("Seeding inventory...");
        await InventorySeedData.SeedAsync(inventoryDb, catalog);

        Console.WriteLine("Seeding a sample sale...");
        await SampleSaleSeedData.SeedAsync(salesDb, inventoryDb, customers, catalog);

        Console.WriteLine("Seeding a sample repair ticket...");
        await SampleRepairSeedData.SeedAsync(repairsDb, customers, technician.Id);
    }
}