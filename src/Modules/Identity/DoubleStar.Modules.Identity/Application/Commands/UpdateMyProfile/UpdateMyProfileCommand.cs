// Identity/Application/Commands/UpdateMyProfileCommand/UpdateMyProfileCommand.cs
namespace DoubleStar.Modules.Identity.Application.Commands.UpdateMyProfileCommand;

public sealed record UpdateMyProfileCommand(string FirstName, string LastName, string? Phone) : IRequest<Result>;