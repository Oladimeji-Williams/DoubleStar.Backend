namespace DoubleStar.Modules.Identity.Application.Commands.ReactivateStaffCommand;

public sealed record ReactivateStaffCommand(Guid UserId) : IRequest<Result>;