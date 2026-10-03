// Identity/Application/Commands/SignInWithEmailCodeCommand/SignInWithEmailCodeCommand.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
namespace DoubleStar.Modules.Identity.Application.Commands.SignInWithEmailCodeCommand;
public sealed record SignInWithEmailCodeCommand(string Email, string Code) : IRequest<Result<AuthenticationResult>>;