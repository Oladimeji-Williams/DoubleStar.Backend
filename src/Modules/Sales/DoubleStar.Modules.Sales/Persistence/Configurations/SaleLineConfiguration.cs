// Persistence/Configurations/SaleLineConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoubleStar.Modules.Sales.Domain.Entities;

namespace DoubleStar.Modules.Sales.Persistence.Configurations;

public sealed class SaleLineConfiguration : IEntityTypeConfiguration<SaleLine>
{
    public void Configure(EntityTypeBuilder<SaleLine> builder)
    {
        builder.ToTable("SaleLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.SerialNumber).HasMaxLength(100);
        builder.HasIndex(l => l.ProductId);
    }
}