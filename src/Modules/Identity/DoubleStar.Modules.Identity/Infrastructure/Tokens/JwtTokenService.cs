// Infrastructure/Tokens/JwtTokenService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using DoubleStar.SharedKernel.Abstractions.Authentication;

namespace DoubleStar.Modules.Identity.Infrastructure.Tokens;

internal sealed class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions _options = options.Value;

    public string GenerateAccessToken(Guid userId, string? email, IEnumerable<string> roles)
    {
        var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, userId.ToString()) };
        if (!string.IsNullOrEmpty(email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));
        }
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer, audience: _options.Audience, claims: claims,
            expires: expires, signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Task<string> GenerateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)));
    }
}