// Persistence/Configurations/RepairStatusHistoryConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoubleStar.Modules.Repairs.Domain.Entities;

namespace DoubleStar.Modules.Repairs.Persistence.Configurations;

public sealed class RepairStatusHistoryConfiguration : IEntityTypeConfiguration<RepairStatusHistory>
{
    public void Configure(EntityTypeBuilder<RepairStatusHistory> builder)
    {
        builder.ToTable("RepairStatusHistory");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.FromStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(h => h.ToStatus).HasConversion<string>().HasMaxLength(20);
    }
}