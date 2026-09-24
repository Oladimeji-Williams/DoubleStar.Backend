// DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resend;
using DoubleStar.SharedKernel.Abstractions.Notifications;
using DoubleStar.BuildingBlocks.Infrastructure.Auditing;
using DoubleStar.Modules.Notifications.Application.Abstractions;
using DoubleStar.Modules.Notifications.Application.Services;
using DoubleStar.Modules.Notifications.Infrastructure.Email;
using DoubleStar.Modules.Notifications.Infrastructure.Sms;
using DoubleStar.Modules.Notifications.Persistence;
using DoubleStar.Modules.Notifications.Persistence.Repositories;

namespace DoubleStar.Modules.Notifications;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<NotificationsDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory_Notifications", "notifications"));
            options.AddInterceptors(sp.GetRequiredService<AuditingInterceptor>());
        });

        services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
        services.AddScoped<NotificationDispatcher>();

        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.AddHttpClient<IResend, ResendClient>();
        services.Configure<ResendClientOptions>(o =>
        {
            o.ApiToken = configuration["Email:ApiKey"]
                ?? throw new InvalidOperationException("Email:ApiKey is not configured.");
        });
        services.AddScoped<IEmailSender, ResendEmailSender>();

        services.Configure<SmsOptions>(configuration.GetSection(SmsOptions.SectionName));
        services.AddHttpClient<ISmsSender, TermiiSmsSender>(httpClient =>
        {
            httpClient.BaseAddress = new Uri("https://v3.api.termii.com/");
        });

        return services;
    }
}