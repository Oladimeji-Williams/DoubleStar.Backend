// DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DoubleStar.SharedKernel.Contracts.Customers;
using DoubleStar.BuildingBlocks.Infrastructure.Auditing;
using DoubleStar.Modules.Customers.Application.Abstractions;
using DoubleStar.Modules.Customers.Application.CrossModule;
using DoubleStar.Modules.Customers.Persistence;
using DoubleStar.Modules.Customers.Persistence.Repositories;

namespace DoubleStar.Modules.Customers;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomersModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<CustomersDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory_Customers", "customers"));
            options.AddInterceptors(sp.GetRequiredService<AuditingInterceptor>());
        });

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerDirectory, CustomerDirectoryService>();

        return services;
    }
}