using System.Reflection;
using DoubleStar.Modules.Catalog;
using DoubleStar.Modules.Customers;
using DoubleStar.Modules.Identity;
using DoubleStar.Modules.Inventory;
using DoubleStar.Modules.Notifications;
using DoubleStar.Modules.Orders;
using DoubleStar.Modules.Payments;
using DoubleStar.Modules.Repairs;
using DoubleStar.Modules.Reviews;
using DoubleStar.Modules.Sales;

namespace DoubleStar.Api;

/// <summary>The one place that lists the modules the API hosts. Add a new module here and nowhere else.</summary>
internal static class ApiModules
{
    public static readonly Assembly[] Assemblies =
    [
        typeof(DoubleStar.Modules.Identity.AssemblyMarker).Assembly,
        typeof(DoubleStar.Modules.Customers.AssemblyMarker).Assembly,
        typeof(DoubleStar.Modules.Catalog.AssemblyMarker).Assembly,
        typeof(DoubleStar.Modules.Inventory.AssemblyMarker).Assembly,
        typeof(DoubleStar.Modules.Sales.AssemblyMarker).Assembly,
        typeof(DoubleStar.Modules.Repairs.AssemblyMarker).Assembly,
        typeof(DoubleStar.Modules.Payments.AssemblyMarker).Assembly,
        typeof(DoubleStar.Modules.Notifications.AssemblyMarker).Assembly,
        typeof(DoubleStar.Modules.Reporting.AssemblyMarker).Assembly,
        typeof(DoubleStar.Modules.Reviews.AssemblyMarker).Assembly,
        typeof(DoubleStar.Modules.Orders.AssemblyMarker).Assembly,
    ];

    public static IServiceCollection AddAllModules(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityModule(configuration);
        services.AddCustomersModule(configuration);
        services.AddCatalogModule(configuration);
        services.AddInventoryModule(configuration);
        services.AddSalesModule(configuration);
        services.AddRepairsModule(configuration);
        services.AddPaymentsModule(configuration);
        services.AddNotificationsModule(configuration);
        services.AddReviewsModule(configuration);
        services.AddOrdersModule(configuration);
        return services;
    }
}