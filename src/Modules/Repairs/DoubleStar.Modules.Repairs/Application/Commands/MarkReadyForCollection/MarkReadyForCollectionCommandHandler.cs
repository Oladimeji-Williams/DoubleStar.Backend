// .../MarkReadyForCollectionCommandHandler.cs
using DoubleStar.SharedKernel.Contracts.Repairs;
using DoubleStar.Modules.Repairs.Application.Abstractions;
using DoubleStar.Modules.Repairs.Application.Errors;

namespace DoubleStar.Modules.Repairs.Application.Commands.MarkReadyForCollectionCommand;

public sealed class MarkReadyForCollectionCommandHandler(IRepairTicketRepository repairTicketRepository, IPublisher publisher)
    : IRequestHandler<MarkReadyForCollectionCommand, Result>
{
    public async Task<Result> Handle(MarkReadyForCollectionCommand request, CancellationToken cancellationToken)
    {
        var ticket = await repairTicketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Failure(RepairErrors.NotFound(request.TicketId));
        }

        var fromStatus = ticket.Status;

        try
        {
            ticket.MarkReadyForCollection();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(RepairErrors.InvalidTransition(ex.Message));
        }

        await repairTicketRepository.UpdateAsync(ticket, cancellationToken);
        await publisher.Publish(
            new RepairStatusChangedEvent(ticket.Id, ticket.CustomerId, fromStatus, ticket.Status), cancellationToken);

        return Result.Success();
    }
}