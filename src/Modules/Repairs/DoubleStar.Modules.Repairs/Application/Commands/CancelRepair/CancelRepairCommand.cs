// Application/Commands/CancelRepairCommand/CancelRepairCommand.cs
namespace DoubleStar.Modules.Repairs.Application.Commands.CancelRepairCommand;

public sealed record CancelRepairCommand(int TicketId) : IRequest<Result>;