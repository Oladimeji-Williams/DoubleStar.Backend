// DependencyInjection.cs
using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using DoubleStar.BuildingBlocks.Application.Behaviors;

namespace DoubleStar.BuildingBlocks.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddBuildingBlocksApplication(
        this IServiceCollection services, params Assembly[] moduleAssemblies)
    {
        services.AddMediatR(configuration =>
        {
            foreach (var assembly in moduleAssemblies)
            {
                configuration.RegisterServicesFromAssembly(assembly);
            }

            // Registered outermost-first: exceptions are caught around everything,
            // validation runs next, logging wraps closest to the actual handler.
            configuration.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        foreach (var assembly in moduleAssemblies)
        {
            services.AddValidatorsFromAssembly(assembly);
        }

        return services;
    }
}