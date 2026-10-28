namespace DoubleStar.Modules.Identity.Application.Commands.RevokeRefreshTokenCommand;

public sealed record RevokeRefreshTokenCommand(string RefreshToken) : IRequest<Result>;