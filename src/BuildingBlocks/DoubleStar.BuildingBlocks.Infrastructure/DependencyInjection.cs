// DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.BuildingBlocks.Infrastructure.Auditing;
using DoubleStar.BuildingBlocks.Infrastructure.Authentication;
using DoubleStar.BuildingBlocks.Infrastructure.Urls;

namespace DoubleStar.BuildingBlocks.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBuildingBlocksInfrastructure(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IUrlBuilder, UrlBuilder>();
        services.AddSingleton<AuditingInterceptor>();

        // Concrete email/SMS/storage/payment providers (Resend, Termii, Cloudinary,
        // Paystack) get wired here module-by-module as we build Notifications and
        // Payments — nothing to register yet, the abstractions just live in SharedKernel.

        return services;
    }
}