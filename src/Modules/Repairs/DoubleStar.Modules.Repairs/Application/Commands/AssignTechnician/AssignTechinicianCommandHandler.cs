// .../AssignTechnicianCommandHandler.cs
using DoubleStar.Modules.Repairs.Application.Abstractions;
using DoubleStar.Modules.Repairs.Application.Errors;

namespace DoubleStar.Modules.Repairs.Application.Commands.AssignTechnicianCommand;

public sealed class AssignTechnicianCommandHandler(IRepairTicketRepository repairTicketRepository)
    : IRequestHandler<AssignTechnicianCommand, Result>
{
    public async Task<Result> Handle(AssignTechnicianCommand request, CancellationToken cancellationToken)
    {
        var ticket = await repairTicketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Failure(RepairErrors.NotFound(request.TicketId));
        }

        try
        {
            ticket.AssignTechnician(request.TechnicianUserId);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(RepairErrors.InvalidTransition(ex.Message));
        }

        await repairTicketRepository.UpdateAsync(ticket, cancellationToken);
        return Result.Success();
    }
}