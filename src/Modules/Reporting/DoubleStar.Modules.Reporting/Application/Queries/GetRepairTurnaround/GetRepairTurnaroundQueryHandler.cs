// .../GetRepairTurnaroundQueryHandler.cs
using DoubleStar.SharedKernel.Contracts.Repairs;

namespace DoubleStar.Modules.Reporting.Application.Queries.GetRepairTurnaroundQuery;

public sealed class GetRepairTurnaroundQueryHandler(IRepairHistory repairHistory)
    : IRequestHandler<GetRepairTurnaroundQuery, Result<RepairTurnaroundDto>>
{
    public async Task<Result<RepairTurnaroundDto>> Handle(GetRepairTurnaroundQuery request, CancellationToken cancellationToken)
    {
        var tickets = await repairHistory.GetCollectedInRangeAsync(request.FromUtc, request.ToUtc, cancellationToken);

        if (tickets.Count == 0)
        {
            return Result<RepairTurnaroundDto>.Success(new RepairTurnaroundDto(0, 0));
        }

        var averageHours = tickets.Average(t => (t.CollectedAtUtc!.Value - t.CreatedAtUtc).TotalHours);

        return Result<RepairTurnaroundDto>.Success(new RepairTurnaroundDto(tickets.Count, Math.Round(averageHours, 1)));
    }
}