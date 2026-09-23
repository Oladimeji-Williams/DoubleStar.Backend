// Persistence/Configurations/SerializedUnitConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoubleStar.Modules.Inventory.Domain.Entities;

namespace DoubleStar.Modules.Inventory.Persistence.Configurations;

public sealed class SerializedUnitConfiguration : IEntityTypeConfiguration<SerializedUnit>
{
    public void Configure(EntityTypeBuilder<SerializedUnit> builder)
    {
        builder.ToTable("SerializedUnits");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.SerialNumber).IsRequired().HasMaxLength(100);
        builder.HasIndex(s => s.SerialNumber).IsUnique();
        builder.HasIndex(s => s.ProductId);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
    }
}