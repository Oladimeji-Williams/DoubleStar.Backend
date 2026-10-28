namespace DoubleStar.Modules.Identity.Application.Commands.UpdateMyProfileCommand;

public sealed record UpdateMyProfileCommand(string FirstName, string LastName, string? Phone) : IRequest<Result>;