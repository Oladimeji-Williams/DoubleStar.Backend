using DoubleStar.Api;
using DoubleStar.Api.Common.Cors;
using DoubleStar.Api.Common.Database;
using DoubleStar.Api.Common.Errors;
using DoubleStar.Api.Common.Health;
using DoubleStar.Api.Common.Security;
using DoubleStar.Api.Realtime;
using DoubleStar.BuildingBlocks.Application;
using DoubleStar.BuildingBlocks.Infrastructure;
using DoubleStar.Modules.Identity.Infrastructure.Identity;
using DoubleStar.SharedKernel.Common.Options;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

if (!string.Equals(
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
    "Testing",
    StringComparison.OrdinalIgnoreCase))
{
    LoadDotEnv();
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration, ApiModules.Assemblies);
builder.Services.AddBuildingBlocksApplication(ApiModules.Assemblies);
builder.Services.AddBuildingBlocksInfrastructure();
builder.Services.AddAllModules(builder.Configuration);
builder.Services.AddRealtimeServices();

builder.Services.Configure<FrontendOptions>(
    builder.Configuration.GetSection(FrontendOptions.SectionName));

builder.Services.AddResponseCompression(options => options.EnableForHttps = true);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services
    .AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);

var app = builder.Build();

// `dotnet DoubleStar.API.dll --migrate-only` applies migrations and exits (for deploy pipelines).
// Setting Database__MigrateOnStartup=true does the same and then keeps serving (single-instance setups).
var migrateOnly = args.Contains("--migrate-only");
if (migrateOnly || app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    await DatabaseMigrator.MigrateAsync(app.Services);
}

if (migrateOnly)
{
    return;
}

using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSecurityHeaders();
app.UseApiCors();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseRateLimiter();
}

app.UseAuthentication();
app.UseAuthorization();

// /health and /health/live: the process is up. /health/ready: the database is reachable too.
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

app.MapControllers();
app.MapHub<RepairTrackingHub>("/hubs/repair-tracking");

app.Run();

static void LoadDotEnv()
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, ".env")))
    {
        directory = directory.Parent;
    }

    // No .env (containers, CI, production): configuration comes from real environment variables.
    if (directory is null)
    {
        return;
    }

    DotNetEnv.Env.Load(Path.Combine(directory.FullName, ".env"));
}

public partial class Program;