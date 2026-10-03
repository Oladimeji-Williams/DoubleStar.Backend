namespace DoubleStar.Modules.Identity.Infrastructure.Identity;

public sealed class EmailSignInCode
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CodeHash { get; set; } = null!;
    public string LinkTokenHash { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? ConsumedAtUtc { get; set; }

    public bool IsActive => ConsumedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;
}