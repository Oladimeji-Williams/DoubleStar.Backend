// Abstractions/Authentication/AuthenticationResult.cs
namespace DoubleStar.SharedKernel.Abstractions.Authentication;

public sealed record AuthenticationResult(
    Guid UserId,
    string? Email,
    string? Phone,
    string DisplayName,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc,
    IReadOnlyList<string> Roles);