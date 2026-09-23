// Application/Commands/RefreshTokenCommand/RefreshTokenCommand.cs
using DoubleStar.Modules.Identity.Application.DTOs;

namespace DoubleStar.Modules.Identity.Application.Commands.RefreshTokenCommand;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthResultDto>>;