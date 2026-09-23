// Application/Commands/LoginCommand/LoginCommand.cs
using DoubleStar.Modules.Identity.Application.DTOs;

namespace DoubleStar.Modules.Identity.Application.Commands.LoginCommand;

public sealed record LoginCommand(string EmailOrPhone, string Password) : IRequest<Result<AuthResultDto>>;