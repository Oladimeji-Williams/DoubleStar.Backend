// DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using DoubleStar.SharedKernel.Abstractions.Payments;
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.BuildingBlocks.Infrastructure.Auditing;
using DoubleStar.Modules.Payments.Application.Abstractions;
using DoubleStar.Modules.Payments.Application.CrossModule;
using DoubleStar.Modules.Payments.Infrastructure.Paystack;
using DoubleStar.Modules.Payments.Persistence;
using DoubleStar.Modules.Payments.Persistence.Repositories;

namespace DoubleStar.Modules.Payments;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<PaymentsDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory_Payments", "payments"));
            options.AddInterceptors(sp.GetRequiredService<AuditingInterceptor>());
        });

        services.Configure<PaystackOptions>(configuration.GetSection(PaystackOptions.SectionName));

        services.AddHttpClient<IPaymentGateway, PaystackService>((sp, httpClient) =>
        {
            var paystackOptions = sp.GetRequiredService<IOptions<PaystackOptions>>().Value;
            httpClient.BaseAddress = new Uri("https://api.paystack.co/");
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", paystackOptions.SecretKey);
        });

        services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
        services.AddScoped<IPaymentRecordReader, PaymentRecordReaderService>();

        return services;
    }
}