// Domain/Entities/RepairStatusHistory.cs
using DoubleStar.SharedKernel.Domain;
using DoubleStar.SharedKernel.Contracts.Repairs;

namespace DoubleStar.Modules.Repairs.Domain.Entities;

public sealed class RepairStatusHistory : Entity
{
    public int RepairTicketId { get; private set; }
    public RepairStatus FromStatus { get; private set; }
    public RepairStatus ToStatus { get; private set; }

    private RepairStatusHistory() { }

    internal static RepairStatusHistory Create(int repairTicketId, RepairStatus from, RepairStatus to) => new()
    {
        RepairTicketId = repairTicketId,
        FromStatus = from,
        ToStatus = to,
    };
}