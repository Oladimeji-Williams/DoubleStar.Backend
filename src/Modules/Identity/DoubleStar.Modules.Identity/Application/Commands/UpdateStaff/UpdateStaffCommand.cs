// Identity/Application/Commands/UpdateStaffCommand/UpdateStaffCommand.cs
namespace DoubleStar.Modules.Identity.Application.Commands.UpdateStaffCommand;

public sealed record UpdateStaffCommand(Guid UserId, string FirstName, string LastName, string Role) : IRequest<Result>;