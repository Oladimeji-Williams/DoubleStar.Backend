// Persistence/PaymentsDbContext.cs
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using DoubleStar.Modules.Payments.Domain.Entities;

namespace DoubleStar.Modules.Payments.Persistence;

public sealed class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : DbContext(options)
{
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<Refund> Refunds => Set<Refund>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("payments");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}