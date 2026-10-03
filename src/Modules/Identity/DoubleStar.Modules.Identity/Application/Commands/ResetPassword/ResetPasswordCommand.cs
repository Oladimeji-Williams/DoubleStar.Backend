// Identity/Application/Commands/ResetPasswordCommand/ResetPasswordCommand.cs
namespace DoubleStar.Modules.Identity.Application.Commands.ResetPasswordCommand;
public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<Result>;