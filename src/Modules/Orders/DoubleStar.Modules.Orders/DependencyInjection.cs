using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DoubleStar.BuildingBlocks.Infrastructure.Auditing;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.Options;
using DoubleStar.Modules.Orders.Application.Services;
using DoubleStar.Modules.Orders.Background;
using DoubleStar.Modules.Orders.Persistence;
using DoubleStar.Modules.Orders.Persistence.Repositories;

namespace DoubleStar.Modules.Orders;

public static class DependencyInjection
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<OrdersDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory_Orders", "orders"));
            options.AddInterceptors(sp.GetRequiredService<AuditingInterceptor>());
        });

        services.AddOptions<OrdersOptions>().BindConfiguration(OrdersOptions.SectionName);

        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddScoped<CartViewBuilder>();
        services.AddScoped<OrderStockService>();
        services.AddScoped<DeliveryFeeCalculator>();

        services.AddHostedService<ExpireUnpaidOrdersService>();

        return services;
    }
}
