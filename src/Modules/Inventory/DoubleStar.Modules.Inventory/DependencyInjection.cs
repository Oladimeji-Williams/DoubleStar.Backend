// DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DoubleStar.SharedKernel.Contracts.Inventory;
using DoubleStar.BuildingBlocks.Infrastructure.Auditing;
using DoubleStar.Modules.Inventory.Application.Abstractions;
using DoubleStar.Modules.Inventory.Application.CrossModule;
using DoubleStar.Modules.Inventory.Persistence;
using DoubleStar.Modules.Inventory.Persistence.Repositories;

namespace DoubleStar.Modules.Inventory;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<InventoryDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory_Inventory", "inventory"));
            options.AddInterceptors(sp.GetRequiredService<AuditingInterceptor>());
        });

        services.AddScoped<IStockItemRepository, StockItemRepository>();
        services.AddScoped<ISerializedUnitRepository, SerializedUnitRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();
        services.AddScoped<IStockLevelReader, StockLevelReaderService>();
        services.AddScoped<IStockAdjuster, StockAdjusterService>();

        return services;
    }
}