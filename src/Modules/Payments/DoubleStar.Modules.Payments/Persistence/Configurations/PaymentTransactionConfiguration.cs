// Persistence/Configurations/PaymentTransactionConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoubleStar.Modules.Payments.Domain.Entities;

namespace DoubleStar.Modules.Payments.Persistence.Configurations;

public sealed class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("PaymentTransactions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.SourceType).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Method).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.PaystackReference).HasMaxLength(100);
        builder.HasIndex(p => p.PaystackReference).IsUnique().HasFilter("\"PaystackReference\" IS NOT NULL");
        builder.HasIndex(p => new { p.SourceType, p.SourceId });

        builder.HasMany(p => p.Refunds).WithOne().HasForeignKey(r => r.PaymentTransactionId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Refunds).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}