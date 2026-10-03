// Identity/Application/DTOs/LoginResponseDto.cs
namespace DoubleStar.Modules.Identity.Application.DTOs;

public sealed record LoginResponseDto(bool RequiresTwoFactor, string? ChallengeToken, AuthResultDto? AuthResult);