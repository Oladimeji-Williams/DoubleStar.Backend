// Domain/Entities/RepairTicket.cs
using DoubleStar.SharedKernel.Domain;
using DoubleStar.SharedKernel.Contracts.Repairs;

namespace DoubleStar.Modules.Repairs.Domain.Entities;

public sealed class RepairTicket : Entity
{
    private static readonly Dictionary<RepairStatus, RepairStatus[]> AllowedTransitions = new()
    {
        [RepairStatus.Received] = [RepairStatus.Diagnosing, RepairStatus.Cancelled],
        [RepairStatus.Diagnosing] = [RepairStatus.AwaitingApproval, RepairStatus.Cancelled],
        [RepairStatus.AwaitingApproval] = [RepairStatus.InRepair, RepairStatus.Cancelled],
        [RepairStatus.InRepair] = [RepairStatus.Ready, RepairStatus.Cancelled],
        [RepairStatus.Ready] = [RepairStatus.Collected, RepairStatus.Cancelled],
        [RepairStatus.Collected] = [],
        [RepairStatus.Cancelled] = [],
    };

    private readonly List<RepairPart> _parts = [];
    private readonly List<RepairStatusHistory> _statusHistory = [];

    public Guid? CustomerId { get; private set; }
    public string DeviceDescription { get; private set; } = null!;
    public string? ImeiOrSerial { get; private set; }
    public string FaultDescription { get; private set; } = null!;
    public string? DiagnosisNotes { get; private set; }
    public long? QuotedPriceKobo { get; private set; }
    public Guid? TechnicianUserId { get; private set; }
    public RepairStatus Status { get; private set; }

    public IReadOnlyList<RepairPart> Parts => _parts;
    public IReadOnlyList<RepairStatusHistory> StatusHistory => _statusHistory;

    private RepairTicket() { }

    public static RepairTicket Open(
        Guid? customerId, string deviceDescription, string? imeiOrSerial, string faultDescription) => new()
    {
        CustomerId = customerId,
        DeviceDescription = deviceDescription.Trim(),
        ImeiOrSerial = imeiOrSerial?.Trim().ToUpperInvariant(),
        FaultDescription = faultDescription.Trim(),
        Status = RepairStatus.Received,
    };

    public void AssignTechnician(Guid technicianUserId)
    {
        EnsureNotTerminal();
        TechnicianUserId = technicianUserId;
    }

    /// <summary>Received/Diagnosing → AwaitingApproval, recording notes and a quote in one step.</summary>
    public void RecordDiagnosis(string diagnosisNotes, long quotedPriceKobo)
    {
        if (Status == RepairStatus.Received)
        {
            TransitionTo(RepairStatus.Diagnosing);
        }

        if (Status != RepairStatus.Diagnosing)
        {
            throw new InvalidOperationException($"Cannot record a diagnosis while the ticket is {Status}.");
        }

        if (quotedPriceKobo < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quotedPriceKobo), "Quote cannot be negative.");
        }

        DiagnosisNotes = diagnosisNotes.Trim();
        QuotedPriceKobo = quotedPriceKobo;
        TransitionTo(RepairStatus.AwaitingApproval);
    }

    public void ApproveQuote()
    {
        if (Status != RepairStatus.AwaitingApproval)
        {
            throw new InvalidOperationException($"Cannot approve a quote while the ticket is {Status}.");
        }
        TransitionTo(RepairStatus.InRepair);
    }

    public RepairPart AddPart(int productId, int quantity, long unitCostKobo)
    {
        if (Status != RepairStatus.InRepair)
        {
            throw new InvalidOperationException("Parts can only be logged while a ticket is InRepair.");
        }

        var part = RepairPart.Create(productId, quantity, unitCostKobo);
        _parts.Add(part);
        return part;
    }

    public void MarkReadyForCollection()
    {
        if (Status != RepairStatus.InRepair)
        {
            throw new InvalidOperationException($"Cannot mark ready while the ticket is {Status}.");
        }
        TransitionTo(RepairStatus.Ready);
    }

    public void CollectDevice()
    {
        if (Status != RepairStatus.Ready)
        {
            throw new InvalidOperationException($"Cannot collect a device while the ticket is {Status}.");
        }
        TransitionTo(RepairStatus.Collected);
    }

    public void Cancel()
    {
        EnsureNotTerminal();
        TransitionTo(RepairStatus.Cancelled);
    }

    private void EnsureNotTerminal()
    {
        if (Status is RepairStatus.Collected or RepairStatus.Cancelled)
        {
            throw new InvalidOperationException($"Ticket is already {Status} and cannot be changed further.");
        }
    }

    private void TransitionTo(RepairStatus newStatus)
    {
        if (!AllowedTransitions[Status].Contains(newStatus))
        {
            throw new InvalidOperationException($"Cannot move a ticket from {Status} to {newStatus}.");
        }

        _statusHistory.Add(RepairStatusHistory.Create(Id, Status, newStatus));
        Status = newStatus;
    }
}