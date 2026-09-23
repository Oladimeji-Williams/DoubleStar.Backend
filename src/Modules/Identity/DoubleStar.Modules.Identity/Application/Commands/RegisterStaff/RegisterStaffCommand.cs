// Application/Commands/RegisterStaffCommand/RegisterStaffCommand.cs
namespace DoubleStar.Modules.Identity.Application.Commands.RegisterStaffCommand;

public sealed record RegisterStaffCommand(
    string FirstName, string LastName, string Email, string Password, string Role) : IRequest<Result<Guid>>;