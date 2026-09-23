// Persistence/RepairsDbContext.cs
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using DoubleStar.Modules.Repairs.Domain.Entities;

namespace DoubleStar.Modules.Repairs.Persistence;

public sealed class RepairsDbContext(DbContextOptions<RepairsDbContext> options) : DbContext(options)
{
    public DbSet<RepairTicket> RepairTickets => Set<RepairTicket>();
    public DbSet<RepairPart> RepairParts => Set<RepairPart>();
    public DbSet<RepairStatusHistory> RepairStatusHistory => Set<RepairStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("repairs");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}