// Persistence/Configurations/StockMovementConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoubleStar.Modules.Inventory.Domain.Entities;

namespace DoubleStar.Modules.Inventory.Persistence.Configurations;

public sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.ProductId);
        builder.Property(s => s.MovementType).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Reference).HasMaxLength(200);
    }
}