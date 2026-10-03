// .../RequestRepairCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.Modules.Repairs.Application.Abstractions;
using DoubleStar.Modules.Repairs.Application.DTOs;
using DoubleStar.Modules.Repairs.Application.Mappings;
using DoubleStar.Modules.Repairs.Domain.Entities;

namespace DoubleStar.Modules.Repairs.Application.Commands.RequestRepairCommand;

public sealed class RequestRepairCommandHandler(IRepairTicketRepository repairTicketRepository, ICurrentUser currentUser)
    : IRequestHandler<RequestRepairCommand, Result<RepairTicketDto>>
{
    public async Task<Result<RepairTicketDto>> Handle(RequestRepairCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            return Result<RepairTicketDto>.Failure(UserErrors.NotAuthenticated());
        }

        var ticket = RepairTicket.Open(currentUser.UserId, request.DeviceDescription, request.ImeiOrSerial, request.FaultDescription);
        await repairTicketRepository.AddAsync(ticket, cancellationToken);

        return Result<RepairTicketDto>.Success(ticket.ToDto());
    }
}