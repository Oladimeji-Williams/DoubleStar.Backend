using DoubleStar.Api;
using DoubleStar.Api.Common.Cors;
using DoubleStar.Api.Common.Security;
using DoubleStar.SharedKernel.Common.Options;
using DoubleStar.BuildingBlocks.Application;
using DoubleStar.BuildingBlocks.Infrastructure;
using DoubleStar.Modules.Identity;
using DoubleStar.Modules.Identity.Infrastructure.Identity;
using DoubleStar.Modules.Customers;
using DoubleStar.Modules.Catalog;
using DoubleStar.Modules.Inventory;


LoadDotEnv();

var builder = WebApplication.CreateBuilder(args);

var moduleAssemblies = new[]
{
    typeof(DoubleStar.Modules.Identity.AssemblyMarker).Assembly,
    typeof(DoubleStar.Modules.Customers.AssemblyMarker).Assembly,
    typeof(DoubleStar.Modules.Catalog.AssemblyMarker).Assembly,
    typeof(DoubleStar.Modules.Inventory.AssemblyMarker).Assembly,

    // more modules get appended here as we build them: Customers, Catalog, ...
};

builder.Services.AddApiServices(builder.Configuration, moduleAssemblies);
builder.Services.AddBuildingBlocksApplication(moduleAssemblies);
builder.Services.AddBuildingBlocksInfrastructure();
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddCustomersModule(builder.Configuration);
builder.Services.AddCatalogModule(builder.Configuration);
builder.Services.AddInventoryModule(builder.Configuration);

builder.Services.Configure<FrontendOptions>(builder.Configuration.GetSection(FrontendOptions.SectionName));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseSecurityHeaders();
app.UseApiCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static void LoadDotEnv()
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, ".env")))
    {
        directory = directory.Parent;
    }

    if (directory is null)
    {
        throw new FileNotFoundException(
            "The repository .env file was not found. Copy .env.example to .env and fill it in.");
    }

    DotNetEnv.Env.Load(Path.Combine(directory.FullName, ".env"));
}

public partial class Program;