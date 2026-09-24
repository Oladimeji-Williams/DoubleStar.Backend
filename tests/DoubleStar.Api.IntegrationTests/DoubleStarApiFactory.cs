using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

using DoubleStar.Modules.Identity.Persistence;
using DoubleStar.Modules.Customers.Persistence;
using DoubleStar.Modules.Catalog.Persistence;
using DoubleStar.Modules.Inventory.Persistence;
using DoubleStar.Modules.Sales.Persistence;
using DoubleStar.Modules.Repairs.Persistence;
using DoubleStar.Modules.Payments.Persistence;
using DoubleStar.Modules.Notifications.Persistence;

namespace DoubleStar.Api.IntegrationTests;

public sealed class DoubleStarApiFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres =
        new PostgreSqlBuilder("postgres:18-alpine")
            .WithDatabase("doublestar_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        using var scope = Services.CreateScope();
        var sp = scope.ServiceProvider;

        await sp.GetRequiredService<ApplicationIdentityDbContext>()
            .Database.MigrateAsync();

        await sp.GetRequiredService<CustomersDbContext>()
            .Database.MigrateAsync();

        await sp.GetRequiredService<CatalogDbContext>()
            .Database.MigrateAsync();

        await sp.GetRequiredService<InventoryDbContext>()
            .Database.MigrateAsync();

        await sp.GetRequiredService<SalesDbContext>()
            .Database.MigrateAsync();

        await sp.GetRequiredService<RepairsDbContext>()
            .Database.MigrateAsync();

        await sp.GetRequiredService<PaymentsDbContext>()
            .Database.MigrateAsync();

        await sp.GetRequiredService<NotificationsDbContext>()
            .Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
        Dispose();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] =
                        _postgres.GetConnectionString(),

                    // JWT
                    ["Jwt:Issuer"] = "DoubleStar.Tests",
                    ["Jwt:Audience"] = "DoubleStar.Tests",
                    ["Jwt:SecretKey"] =
                        "integration-test-secret-key-needs-to-be-long-enough-for-hmac",
                    ["Jwt:AccessTokenExpirationMinutes"] = "15",
                    ["Jwt:RefreshTokenExpirationDays"] = "7",

                    // Frontend
                    ["Frontend:BaseUrl"] =
                        "http://localhost:4200",

                    // CORS
                    ["Cors:AllowedOrigins:0"] =
                        "http://localhost:4200",

                    // Email
                    ["Email:FromAddress"] =
                        "test@doublestar.local",
                    ["Email:FromName"] =
                        "DoubleStar Test",
                    ["Email:ApiKey"] =
                        "re_test_dummy",

                    // SMS
                    ["Sms:ApiKey"] =
                        "test_dummy",
                    ["Sms:SenderId"] =
                        "DoubleStar",

                    // Payments
                    ["Paystack:SecretKey"] =
                        "sk_test_dummy",

                    ["AllowedHosts"] = "*"
                });
        });
    }
}