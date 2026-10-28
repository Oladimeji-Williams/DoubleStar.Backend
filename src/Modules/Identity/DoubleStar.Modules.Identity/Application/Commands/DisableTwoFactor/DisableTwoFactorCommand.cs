namespace DoubleStar.Modules.Identity.Application.Commands.DisableTwoFactorCommand;
public sealed record DisableTwoFactorCommand(string CurrentPassword) : IRequest<Result>;