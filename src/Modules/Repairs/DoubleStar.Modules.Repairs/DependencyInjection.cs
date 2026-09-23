// DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DoubleStar.SharedKernel.Contracts.Repairs;
using DoubleStar.BuildingBlocks.Infrastructure.Auditing;
using DoubleStar.Modules.Repairs.Application.Abstractions;
using DoubleStar.Modules.Repairs.Application.CrossModule;
using DoubleStar.Modules.Repairs.Persistence;
using DoubleStar.Modules.Repairs.Persistence.Repositories;

namespace DoubleStar.Modules.Repairs;

public static class DependencyInjection
{
    public static IServiceCollection AddRepairsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<RepairsDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory_Repairs", "repairs"));
            options.AddInterceptors(sp.GetRequiredService<AuditingInterceptor>());
        });

        services.AddScoped<IRepairTicketRepository, RepairTicketRepository>();
        services.AddScoped<IRepairHistory, RepairHistoryService>();

        return services;
    }
}