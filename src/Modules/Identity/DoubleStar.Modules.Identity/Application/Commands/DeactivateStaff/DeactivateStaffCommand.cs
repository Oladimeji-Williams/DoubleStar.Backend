// Application/Commands/DeactivateStaffCommand/DeactivateStaffCommand.cs
namespace DoubleStar.Modules.Identity.Application.Commands.DeactivateStaffCommand;

public sealed record DeactivateStaffCommand(Guid UserId) : IRequest<Result>;