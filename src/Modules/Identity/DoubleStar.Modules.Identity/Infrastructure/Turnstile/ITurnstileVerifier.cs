// Identity/Infrastructure/Turnstile/ITurnstileVerifier.cs
namespace DoubleStar.Modules.Identity.Infrastructure.Turnstile;

public interface ITurnstileVerifier
{
    Task<bool> VerifyAsync(string? token, CancellationToken cancellationToken);
}