// Program.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DoubleStar.BuildingBlocks.Infrastructure;
using DoubleStar.Modules.Identity;
using DoubleStar.Modules.Customers;
using DoubleStar.Modules.Catalog;
using DoubleStar.Modules.Inventory;
using DoubleStar.Modules.Sales;
using DoubleStar.Modules.Repairs;
using DoubleStar.Modules.Payments;
using DoubleStar.Modules.Notifications;
using DoubleStar.DatabaseSeeder;

LoadDotEnv();

var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();

var services = new ServiceCollection();
services.AddBuildingBlocksInfrastructure();
services.AddIdentityModule(configuration);
services.AddCustomersModule(configuration);
services.AddCatalogModule(configuration);
services.AddInventoryModule(configuration);
services.AddSalesModule(configuration);
services.AddRepairsModule(configuration);
services.AddPaymentsModule(configuration);
services.AddNotificationsModule(configuration);

await using var serviceProvider = services.BuildServiceProvider();
await DatabaseInitializer.InitializeAsync(serviceProvider);

Console.WriteLine();
Console.WriteLine("Database reset and seeded successfully.");

static void LoadDotEnv()
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, ".env")))
    {
        directory = directory.Parent;
    }

    if (directory is null)
    {
        throw new FileNotFoundException("The repository .env file was not found. Copy .env.example to .env and fill it in.");
    }

    DotNetEnv.Env.Load(Path.Combine(directory.FullName, ".env"));
}