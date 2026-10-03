// Identity/Application/Commands/ConfirmEmailCommand/ConfirmEmailCommand.cs
namespace DoubleStar.Modules.Identity.Application.Commands.ConfirmEmailCommand;
public sealed record ConfirmEmailCommand(string Email, string Token) : IRequest<Result>;