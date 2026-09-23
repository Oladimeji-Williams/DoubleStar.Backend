// Persistence/Configurations/RepairPartConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoubleStar.Modules.Repairs.Domain.Entities;

namespace DoubleStar.Modules.Repairs.Persistence.Configurations;

public sealed class RepairPartConfiguration : IEntityTypeConfiguration<RepairPart>
{
    public void Configure(EntityTypeBuilder<RepairPart> builder)
    {
        builder.ToTable("RepairParts");
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.ProductId);
    }
}