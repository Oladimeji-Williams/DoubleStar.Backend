// Persistence/Configurations/NotificationLogConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoubleStar.Modules.Notifications.Domain.Entities;

namespace DoubleStar.Modules.Notifications.Persistence.Configurations;

public sealed class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.ToTable("NotificationLog");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Channel).HasConversion<string>().HasMaxLength(20);
        builder.Property(n => n.Recipient).IsRequired().HasMaxLength(256);
        builder.Property(n => n.TemplateName).IsRequired().HasMaxLength(100);
        builder.Property(n => n.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(n => n.ErrorMessage).HasMaxLength(1000);
    }
}