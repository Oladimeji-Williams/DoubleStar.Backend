// Application/DTOs/AuthResultDto.cs
namespace DoubleStar.Modules.Identity.Application.DTOs;

public sealed record AuthResultDto(
    string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAtUtc, DateTime RefreshTokenExpiresAtUtc,
    Guid UserId, string? Email, string? Phone, string DisplayName, IReadOnlyList<string> Roles);