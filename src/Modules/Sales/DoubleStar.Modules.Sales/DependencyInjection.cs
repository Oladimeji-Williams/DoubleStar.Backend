using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DoubleStar.SharedKernel.Contracts.Sales;
using DoubleStar.BuildingBlocks.Infrastructure.Auditing;
using DoubleStar.Modules.Sales.Application.Abstractions;
using DoubleStar.Modules.Sales.Application.CrossModule;
using DoubleStar.Modules.Sales.Persistence;
using DoubleStar.Modules.Sales.Persistence.Repositories;
using DoubleStar.Modules.Sales.Persistence.Analytics;

namespace DoubleStar.Modules.Sales;

public static class DependencyInjection
{
    public static IServiceCollection AddSalesModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<SalesDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory_Sales", "sales"));
            options.AddInterceptors(sp.GetRequiredService<AuditingInterceptor>());
        });

        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<ISalesHistory, SalesHistoryService>();
        services.AddScoped<ISalesAnalyticsReader, SalesAnalyticsReader>();

        return services;
    }
}