// Identity/Infrastructure/Turnstile/TurnstileOptions.cs
namespace DoubleStar.Modules.Identity.Infrastructure.Turnstile;

public sealed class TurnstileOptions
{
    public const string SectionName = "Turnstile";
    public string SecretKey { get; init; } = null!;
}