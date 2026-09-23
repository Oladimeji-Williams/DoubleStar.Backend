// .../RecordDiagnosisCommandHandler.cs
using DoubleStar.SharedKernel.Contracts.Repairs;
using DoubleStar.Modules.Repairs.Application.Abstractions;
using DoubleStar.Modules.Repairs.Application.Errors;

namespace DoubleStar.Modules.Repairs.Application.Commands.RecordDiagnosisCommand;

public sealed class RecordDiagnosisCommandHandler(IRepairTicketRepository repairTicketRepository, IPublisher publisher)
    : IRequestHandler<RecordDiagnosisCommand, Result>
{
    public async Task<Result> Handle(RecordDiagnosisCommand request, CancellationToken cancellationToken)
    {
        var ticket = await repairTicketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Failure(RepairErrors.NotFound(request.TicketId));
        }

        var fromStatus = ticket.Status;

        try
        {
            ticket.RecordDiagnosis(request.DiagnosisNotes, request.QuotedPriceKobo);
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