using System.Reflection;
using DoubleStar.Api.Common.Cors;
using DoubleStar.Api.Common.RateLimiting;
using DoubleStar.Api.Common.Versioning;
using DoubleStar.BuildingBlocks.Infrastructure.Api;

namespace DoubleStar.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services, IConfiguration configuration, params Assembly[] moduleControllerAssemblies)
    {
        services.AddApiControllers(moduleControllerAssemblies);
        services.AddApiVersioningSetup();
        services.AddOpenApi();
        services.AddApiCors(configuration);
        services.AddApiRateLimiting();

        services.AddSingleton(TimeProvider.System);

        return services;
    }
}