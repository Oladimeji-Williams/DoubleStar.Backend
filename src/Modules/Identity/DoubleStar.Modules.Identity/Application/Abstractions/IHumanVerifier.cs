namespace DoubleStar.Modules.Identity.Application.Abstractions;

/// <summary>Proves a request comes from a human. Implemented in Infrastructure.</summary>
public interface IHumanVerifier
{
    Task<bool> VerifyAsync(string? token, CancellationToken cancellationToken);
}