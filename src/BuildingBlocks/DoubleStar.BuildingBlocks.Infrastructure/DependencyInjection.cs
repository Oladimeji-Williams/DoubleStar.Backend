// DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.BuildingBlocks.Infrastructure.Auditing;
using DoubleStar.BuildingBlocks.Infrastructure.Authentication;
using DoubleStar.BuildingBlocks.Infrastructure.Urls;
using Cloudinary = CloudinaryDotNet.Cloudinary;
using Account = CloudinaryDotNet.Account;
using DoubleStar.SharedKernel.Abstractions.Storage;
using DoubleStar.BuildingBlocks.Infrastructure.Storage;
using Microsoft.Extensions.Options;


namespace DoubleStar.BuildingBlocks.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBuildingBlocksInfrastructure(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IUrlBuilder, UrlBuilder>();
        services.AddSingleton<AuditingInterceptor>();
        services.AddOptions<CloudinaryOptions>().BindConfiguration(CloudinaryOptions.SectionName);
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<CloudinaryOptions>>().Value;
            var account = new Account(options.CloudName, options.ApiKey, options.ApiSecret);
            return new Cloudinary(account);
        });
        services.AddSingleton<IFileStorageService, CloudinaryFileStorageService>();
        services.AddSingleton<IImageValidator, ImageValidator>();

        // Concrete email/SMS/storage/payment providers (Resend, Termii, Cloudinary,
        // Paystack) get wired here module-by-module as we build Notifications and
        // Payments — nothing to register yet, the abstractions just live in SharedKernel.

        return services;
    }
}