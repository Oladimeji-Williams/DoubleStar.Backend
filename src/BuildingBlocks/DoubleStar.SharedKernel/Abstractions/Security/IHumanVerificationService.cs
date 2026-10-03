namespace DoubleStar.SharedKernel.Abstractions.Security;

public interface IHumanVerificationService
{
    Task<(Guid ChallengeId, int IconCount, int CorrectIndex)> CreateChallengeAsync(CancellationToken cancellationToken);
    Task<bool> VerifyAsync(Guid challengeId, int selectedIndex, CancellationToken cancellationToken);
    Task<bool> IsVerifiedAsync(Guid challengeId, CancellationToken cancellationToken);
}