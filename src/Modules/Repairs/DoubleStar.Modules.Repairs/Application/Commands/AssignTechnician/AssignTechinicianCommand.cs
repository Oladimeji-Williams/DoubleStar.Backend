// Application/Commands/AssignTechnicianCommand/AssignTechnicianCommand.cs
namespace DoubleStar.Modules.Repairs.Application.Commands.AssignTechnicianCommand;

public sealed record AssignTechnicianCommand(int TicketId, Guid TechnicianUserId) : IRequest<Result>;