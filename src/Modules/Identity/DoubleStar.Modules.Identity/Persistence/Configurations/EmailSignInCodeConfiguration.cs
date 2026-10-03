using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoubleStar.Modules.Identity.Infrastructure.Identity;

namespace DoubleStar.Modules.Identity.Persistence.Configurations;

internal sealed class EmailSignInCodeConfiguration : IEntityTypeConfiguration<EmailSignInCode>
{
    public void Configure(EntityTypeBuilder<EmailSignInCode> builder)
    {
        builder.ToTable("EmailSignInCodes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CodeHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => x.UserId);
    }
}