// Application/Commands/OpenRepairTicketCommand/OpenRepairTicketCommand.cs
using DoubleStar.Modules.Repairs.Application.DTOs;

namespace DoubleStar.Modules.Repairs.Application.Commands.OpenRepairTicketCommand;

public sealed record OpenRepairTicketCommand(
    Guid? CustomerId, string? WalkInName, string? WalkInPhone,
    string DeviceDescription, string? ImeiOrSerial, string FaultDescription) : IRequest<Result<RepairTicketDto>>;