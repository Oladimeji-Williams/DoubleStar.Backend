// Api/ControllerExtensions.cs
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace DoubleStar.BuildingBlocks.Infrastructure.Api;

public static class ControllerExtensions
{
    public static IServiceCollection AddApiControllers(
        this IServiceCollection services, params Assembly[] moduleAssemblies)
    {
        var builder = services.AddControllers()
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        foreach (var assembly in moduleAssemblies)
        {
            builder.AddApplicationPart(assembly);
        }

        return services;
    }
}