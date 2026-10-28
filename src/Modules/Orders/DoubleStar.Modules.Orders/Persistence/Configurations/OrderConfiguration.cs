using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoubleStar.Modules.Orders.Domain.Entities;

namespace DoubleStar.Modules.Orders.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(o => o.FulfillmentMethod).HasConversion<string>().HasMaxLength(20);

        builder.Property(o => o.ContactName).IsRequired().HasMaxLength(200);
        builder.Property(o => o.ContactPhone).IsRequired().HasMaxLength(20);
        builder.Property(o => o.DeliveryAddress).HasMaxLength(500);
        builder.Property(o => o.DeliveryCity).HasMaxLength(100);
        builder.Property(o => o.DeliveryState).HasMaxLength(100);
        builder.Property(o => o.Notes).HasMaxLength(500);
        builder.Property(o => o.IdempotencyKey).HasMaxLength(100);
        builder.Property(o => o.PaymentReference).HasMaxLength(100);
        builder.Property(o => o.AttentionReason).HasMaxLength(500);

        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.ExpiresAtUtc);

        // Same customer + same key = same order. NULL keys are exempt.
        builder.HasIndex(o => new { o.CustomerId, o.IdempotencyKey })
            .IsUnique()
            .HasFilter("\"IdempotencyKey\" IS NOT NULL");

        builder.HasMany(o => o.Lines).WithOne().HasForeignKey(l => l.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(o => o.StatusHistory).WithOne().HasForeignKey(h => h.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.StatusHistory).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("OrderLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(l => l.Sku).IsRequired().HasMaxLength(50);

        // Npgsql maps string[] to a native Postgres text[].
        builder.Property(l => l.SerialNumbers).HasColumnType("text[]");

        builder.HasIndex(l => l.ProductId);
    }
}

public sealed class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("OrderStatusHistory");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.FromStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(h => h.ToStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(h => h.Note).HasMaxLength(500);
    }
}
