namespace DoubleStar.Modules.Identity.Application.Commands.SignInWithEmailLinkCommand;
public sealed record SignInWithEmailLinkCommand(string Token) : IRequest<Result<Application.DTOs.AuthResultDto>>;