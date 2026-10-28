using DoubleStar.Modules.Catalog.Persistence;
using DoubleStar.Modules.Customers.Persistence;
using DoubleStar.Modules.Identity.Persistence;
using DoubleStar.Modules.Inventory.Persistence;
using DoubleStar.Modules.Notifications.Persistence;
using DoubleStar.Modules.Orders.Persistence;
using DoubleStar.Modules.Payments.Persistence;
using DoubleStar.Modules.Repairs.Persistence;
using DoubleStar.Modules.Reviews.Persistence;
using DoubleStar.Modules.Sales.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DoubleStar.Api.Common.Database;

/// <summary>
/// Applies every module's pending EF Core migrations. Non-destructive: it only moves the schema forward.
/// All modules share one PostgreSQL database, each in its own schema.
/// </summary>
public static class DatabaseMigrator
{
    public static async Task MigrateAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DatabaseMigrator));

        await MigrateAsync<ApplicationIdentityDbContext>(provider, logger, cancellationToken);
        await MigrateAsync<CustomersDbContext>(provider, logger, cancellationToken);
        await MigrateAsync<CatalogDbContext>(provider, logger, cancellationToken);
        await MigrateAsync<InventoryDbContext>(provider, logger, cancellationToken);
        await MigrateAsync<SalesDbContext>(provider, logger, cancellationToken);
        await MigrateAsync<RepairsDbContext>(provider, logger, cancellationToken);
        await MigrateAsync<PaymentsDbContext>(provider, logger, cancellationToken);
        await MigrateAsync<NotificationsDbContext>(provider, logger, cancellationToken);
        await MigrateAsync<ReviewsDbContext>(provider, logger, cancellationToken);
        await MigrateAsync<OrdersDbContext>(provider, logger, cancellationToken);
    }

    private static async Task MigrateAsync<TContext>(
        IServiceProvider provider, ILogger logger, CancellationToken cancellationToken)
        where TContext : DbContext
    {
        var context = provider.GetRequiredService<TContext>();
        var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

        if (pending.Count == 0)
        {
            logger.LogInformation("{Context}: schema is up to date", typeof(TContext).Name);
            return;
        }

        logger.LogInformation("{Context}: applying {Count} migration(s)", typeof(TContext).Name, pending.Count);
        await context.Database.MigrateAsync(cancellationToken);
    }
}