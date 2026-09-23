// Application/Commands/ChangePasswordCommand/ChangePasswordCommand.cs
namespace DoubleStar.Modules.Identity.Application.Commands.ChangePasswordCommand;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest<Result>;