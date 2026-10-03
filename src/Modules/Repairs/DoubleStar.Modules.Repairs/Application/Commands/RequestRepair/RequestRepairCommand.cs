// Repairs/Application/Commands/RequestRepairCommand/RequestRepairCommand.cs
using DoubleStar.Modules.Repairs.Application.DTOs;

namespace DoubleStar.Modules.Repairs.Application.Commands.RequestRepairCommand;

public sealed record RequestRepairCommand(string DeviceDescription, string? ImeiOrSerial, string FaultDescription)
    : IRequest<Result<RepairTicketDto>>;