namespace DoubleStar.Modules.Identity.Infrastructure.Identity;

public sealed class HumanVerificationChallenge
{
    public Guid Id { get; set; }
    public int CorrectIndex { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }

    public bool IsVerified => VerifiedAtUtc is not null;
}