// Modules/Reviews/DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DoubleStar.Modules.Reviews.Application.Abstractions;
using DoubleStar.Modules.Reviews.Persistence;
using DoubleStar.Modules.Reviews.Persistence.Repositories;

namespace DoubleStar.Modules.Reviews;

public static class DependencyInjection
{
    public static IServiceCollection AddReviewsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ReviewsDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory_Reviews", "reviews")));

        services.AddScoped<IReviewRepository, ReviewRepository>();
        return services;
    }
}