// Persistence/Configurations/ProductConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoubleStar.Modules.Catalog.Domain.Entities;

namespace DoubleStar.Modules.Catalog.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Sku).IsRequired().HasMaxLength(50);
        builder.HasIndex(p => p.Sku).IsUnique();
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.TrackingMode).HasConversion<string>().HasMaxLength(20);
        builder.HasOne<Category>().WithMany().HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<Brand>().WithMany().HasForeignKey(p => p.BrandId).OnDelete(DeleteBehavior.SetNull);
    }
}