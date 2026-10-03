// Identity/Infrastructure/Identity/TwoFactorChallenge.cs
namespace DoubleStar.Modules.Identity.Infrastructure.Identity;

public sealed class TwoFactorChallenge
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? ConsumedAtUtc { get; set; }

    public bool IsActive => ConsumedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;
}