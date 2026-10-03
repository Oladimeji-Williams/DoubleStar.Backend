// Identity/Application/Commands/DisableTwoFactorCommand/DisableTwoFactorCommand.cs
namespace DoubleStar.Modules.Identity.Application.Commands.DisableTwoFactorCommand;
public sealed record DisableTwoFactorCommand(string CurrentPassword) : IRequest<Result>;