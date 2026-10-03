using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Abstractions.Security;

using DoubleStar.BuildingBlocks.Infrastructure.Auditing;

using DoubleStar.Modules.Identity.Infrastructure.Identity;
using DoubleStar.Modules.Identity.Infrastructure.Security;
using DoubleStar.Modules.Identity.Infrastructure.Tokens;
using DoubleStar.Modules.Identity.Persistence;
using DoubleStar.Modules.Identity.Infrastructure.Turnstile;

namespace DoubleStar.Modules.Identity;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");

        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName);

        services.AddDbContext<ApplicationIdentityDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                connectionString,
                npgsql =>
                {
                    npgsql.MigrationsHistoryTable(
                        "__EFMigrationsHistory_Identity",
                        "identity");
                });

            options.AddInterceptors(
                sp.GetRequiredService<AuditingInterceptor>());
        });

        services.AddIdentityConfiguration();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        // wherever AddIdentityModule registers services — add these two lines
        services.AddOptions<TurnstileOptions>().BindConfiguration(TurnstileOptions.SectionName);
        services.AddHttpClient<ITurnstileVerifier, TurnstileVerifier>();
        // Human verification
        services.AddScoped<IHumanVerificationService, HumanVerificationService>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>(
                (options, jwtOptions) =>
                {
                    var jwt = jwtOptions.Value;

                    if (string.IsNullOrWhiteSpace(jwt.Issuer))
                    {
                        throw new InvalidOperationException(
                            "JWT Issuer is not configured.");
                    }

                    if (string.IsNullOrWhiteSpace(jwt.Audience))
                    {
                        throw new InvalidOperationException(
                            "JWT Audience is not configured.");
                    }

                    if (string.IsNullOrWhiteSpace(jwt.SecretKey))
                    {
                        throw new InvalidOperationException(
                            "JWT SecretKey is not configured.");
                    }

                    var key = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt.SecretKey));

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = jwt.Issuer,

                            ValidateAudience = true,
                            ValidAudience = jwt.Audience,

                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = key,

                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero
                        };
                });

        services.AddAuthorization();

        return services;
    }
}