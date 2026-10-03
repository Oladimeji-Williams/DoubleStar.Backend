// Identity/Infrastructure/Identity/HumanVerificationService.cs
using Microsoft.EntityFrameworkCore;
using DoubleStar.Modules.Identity.Persistence;
using DoubleStar.SharedKernel.Abstractions.Security;
using DoubleStar.Modules.Identity.Infrastructure.Identity;

namespace DoubleStar.Modules.Identity.Infrastructure.Security;

internal sealed class HumanVerificationService(ApplicationIdentityDbContext dbContext) : IHumanVerificationService
{
    private const int IconCount = 6;
    private const int MinimumSolveMilliseconds = 500;

    public async Task<(Guid, int, int)> CreateChallengeAsync(CancellationToken cancellationToken)
    {
        var correctIndex = Random.Shared.Next(0, IconCount);
        var challenge = new HumanVerificationChallenge
        {
            Id = Guid.NewGuid(),
            CorrectIndex = correctIndex,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddSeconds(60),
        };
        await dbContext.HumanVerificationChallenges.AddAsync(challenge, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return (challenge.Id, IconCount, correctIndex);
    }

    public async Task<bool> VerifyAsync(Guid challengeId, int selectedIndex, CancellationToken cancellationToken)
    {
        var challenge = await dbContext.HumanVerificationChallenges.FirstOrDefaultAsync(c => c.Id == challengeId, cancellationToken);
        if (challenge is null || challenge.IsVerified || challenge.ExpiresAtUtc < DateTime.UtcNow)
        {
            return false;
        }

        var solvedTooFast = (DateTime.UtcNow - challenge.CreatedAtUtc).TotalMilliseconds < MinimumSolveMilliseconds;
        if (solvedTooFast || selectedIndex != challenge.CorrectIndex)
        {
            return false;
        }

        challenge.VerifiedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> IsVerifiedAsync(Guid challengeId, CancellationToken cancellationToken)
    {
        var challenge = await dbContext.HumanVerificationChallenges.FirstOrDefaultAsync(c => c.Id == challengeId, cancellationToken);
        return challenge is not null && challenge.IsVerified && (DateTime.UtcNow - challenge.VerifiedAtUtc!.Value).TotalMinutes < 10;
    }
}