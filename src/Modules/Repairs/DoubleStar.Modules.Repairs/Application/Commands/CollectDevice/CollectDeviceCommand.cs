namespace DoubleStar.Modules.Repairs.Application.Commands.CollectDeviceCommand;

public sealed record CollectDeviceCommand(int TicketId) : IRequest<Result>;