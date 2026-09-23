// Abstractions/Authentication/ITokenService.cs
namespace DoubleStar.SharedKernel.Abstractions.Authentication;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string? email, IEnumerable<string> roles);
    Task<string> GenerateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken);
}