// Persistence/SalesDbContext.cs
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using DoubleStar.Modules.Sales.Domain.Entities;

namespace DoubleStar.Modules.Sales.Persistence;

public sealed class SalesDbContext(DbContextOptions<SalesDbContext> options) : DbContext(options)
{
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleLine> SaleLines => Set<SaleLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("sales");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}