// Application/Commands/AddPartUsedCommand/AddPartUsedCommand.cs
using DoubleStar.Modules.Repairs.Application.DTOs;

namespace DoubleStar.Modules.Repairs.Application.Commands.AddPartUsedCommand;

public sealed record AddPartUsedCommand(int TicketId, int ProductId, int Quantity) : IRequest<Result<RepairPartDto>>;