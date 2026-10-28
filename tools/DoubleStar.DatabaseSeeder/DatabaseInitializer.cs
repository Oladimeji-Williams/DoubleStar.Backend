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
using DoubleStar.Modules.Reviews.Persistence;
using DoubleStar.DatabaseSeeder.SeedData;
using DoubleStar.Modules.Orders.Persistence;


namespace DoubleStar.DatabaseSeeder;

/// <summary>
/// Destructive development database reset.
/// Deletes the entire development database, recreates it through
/// EF Core migrations, and then seeds development data.
/// NEVER use against a production database.
/// </summary>
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
        var reviewsDb = sp.GetRequiredService<ReviewsDbContext>();
        var ordersDb = sp.GetRequiredService<OrdersDbContext>();


        // IMPORTANT:
        // All module DbContexts point to the same PostgreSQL database.
        // Therefore, EnsureDeletedAsync() must only be called ONCE.
        Console.WriteLine("Deleting development database...");
        await identityDb.Database.EnsureDeletedAsync();

        Console.WriteLine("Applying migrations...");

        await identityDb.Database.MigrateAsync();
        await customersDb.Database.MigrateAsync();
        await reviewsDb.Database.MigrateAsync();
        await catalogDb.Database.MigrateAsync();
        await inventoryDb.Database.MigrateAsync();
        await salesDb.Database.MigrateAsync();
        await repairsDb.Database.MigrateAsync();
        await paymentsDb.Database.MigrateAsync();
        await notificationsDb.Database.MigrateAsync();
        await ordersDb.Database.MigrateAsync();

        Console.WriteLine("Seeding roles and bootstrap admin...");
        await IdentitySeeder.SeedAsync(sp);

        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

        Console.WriteLine("Seeding staff...");
        var (manager, _, technician) =
            await StaffSeedData.SeedAsync(userManager);

        Console.WriteLine("Seeding customers...");
        var customers = await CustomerSeedData.SeedAsync(customersDb);

        Console.WriteLine("Seeding catalog...");
        var catalog = await CatalogSeedData.SeedAsync(catalogDb);

        Console.WriteLine("Seeding inventory...");
        await InventorySeedData.SeedAsync(inventoryDb, catalog);

        Console.WriteLine("Seeding a sample sale...");
        await SaleSeedData.SeedAsync(
            salesDb,
            inventoryDb,
            customers,
            catalog);

        Console.WriteLine("Seeding a sample repair ticket...");
        await RepairSeedData.SeedAsync(
            repairsDb,
            customers,
            technician.Id);

        Console.WriteLine("Seeding reviews...");
        await ReviewSeedData.SeedAsync(
            reviewsDb,
            manager.Id);

        Console.WriteLine("Development database initialization completed successfully.");
    }
}