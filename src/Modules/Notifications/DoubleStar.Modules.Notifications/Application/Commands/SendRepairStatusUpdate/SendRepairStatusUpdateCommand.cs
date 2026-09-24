// Application/Commands/SendRepairStatusUpdateCommand/SendRepairStatusUpdateCommand.cs
namespace DoubleStar.Modules.Notifications.Application.Commands.SendRepairStatusUpdateCommand;

public sealed record SendRepairStatusUpdateCommand(int TicketId) : IRequest<Result>;