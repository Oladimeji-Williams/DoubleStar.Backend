// Identity/Persistence/Configurations/HumanVerificationChallengeConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoubleStar.Modules.Identity.Infrastructure.Identity;

namespace DoubleStar.Modules.Identity.Persistence.Configurations;

internal sealed class HumanVerificationChallengeConfiguration : IEntityTypeConfiguration<HumanVerificationChallenge>
{
    public void Configure(EntityTypeBuilder<HumanVerificationChallenge> builder)
    {
        builder.ToTable("HumanVerificationChallenges");
        builder.HasKey(x => x.Id);
    }
}