// Identity/Application/Commands/RequestEmailSignInCodeCommand/RequestEmailSignInCodeCommand.cs
namespace DoubleStar.Modules.Identity.Application.Commands.RequestEmailSignInCodeCommand;
public sealed record RequestEmailSignInCodeCommand(string Email) : IRequest<Result>;