// .../OpenRepairTicketCommandHandler.cs
using DoubleStar.SharedKernel.Contracts.Customers;
using DoubleStar.Modules.Repairs.Application.Abstractions;
using DoubleStar.Modules.Repairs.Application.DTOs;
using DoubleStar.Modules.Repairs.Application.Mappings;
using DoubleStar.Modules.Repairs.Domain.Entities;

namespace DoubleStar.Modules.Repairs.Application.Commands.OpenRepairTicketCommand;

public sealed class OpenRepairTicketCommandHandler(
    IRepairTicketRepository repairTicketRepository, ICustomerDirectory customerDirectory)
    : IRequestHandler<OpenRepairTicketCommand, Result<RepairTicketDto>>
{
    public async Task<Result<RepairTicketDto>> Handle(OpenRepairTicketCommand request, CancellationToken cancellationToken)
    {
        Guid? customerId = request.CustomerId;

        if (customerId is null && !string.IsNullOrWhiteSpace(request.WalkInName))
        {
            var walkIn = await customerDirectory.GetOrCreateWalkInAsync(
                request.WalkInName, request.WalkInPhone, cancellationToken);
            customerId = walkIn.Id;
        }

        var ticket = RepairTicket.Open(customerId, request.DeviceDescription, request.ImeiOrSerial, request.FaultDescription);
        await repairTicketRepository.AddAsync(ticket, cancellationToken);

        return Result<RepairTicketDto>.Success(ticket.ToDto());
    }
}