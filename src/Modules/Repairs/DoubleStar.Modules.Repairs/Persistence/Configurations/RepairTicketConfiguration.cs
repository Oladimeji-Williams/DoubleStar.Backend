// Persistence/Configurations/RepairTicketConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoubleStar.Modules.Repairs.Domain.Entities;

namespace DoubleStar.Modules.Repairs.Persistence.Configurations;

public sealed class RepairTicketConfiguration : IEntityTypeConfiguration<RepairTicket>
{
    public void Configure(EntityTypeBuilder<RepairTicket> builder)
    {
        builder.ToTable("RepairTickets");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.DeviceDescription).IsRequired().HasMaxLength(200);
        builder.Property(t => t.ImeiOrSerial).HasMaxLength(100);
        builder.Property(t => t.FaultDescription).IsRequired().HasMaxLength(2000);
        builder.Property(t => t.DiagnosisNotes).HasMaxLength(2000);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(t => t.CustomerId);
        builder.HasIndex(t => t.Status);

        builder.HasMany(t => t.Parts).WithOne().HasForeignKey(p => p.RepairTicketId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(t => t.Parts).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(t => t.StatusHistory).WithOne().HasForeignKey(h => h.RepairTicketId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(t => t.StatusHistory).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}