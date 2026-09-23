// Application/Mappings/AuthenticationMappings.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.Modules.Identity.Application.DTOs;

namespace DoubleStar.Modules.Identity.Application.Mappings;

public static class AuthenticationMappings
{
    public static AuthResultDto ToDto(this AuthenticationResult result) => new(
        result.AccessToken, result.RefreshToken, result.AccessTokenExpiresAtUtc, result.RefreshTokenExpiresAtUtc,
        result.UserId, result.Email, result.Phone, result.DisplayName, result.Roles);
}