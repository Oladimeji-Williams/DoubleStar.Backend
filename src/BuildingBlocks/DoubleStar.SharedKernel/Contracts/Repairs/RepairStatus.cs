// Contracts/Repairs/RepairStatus.cs
namespace DoubleStar.SharedKernel.Contracts.Repairs;

public enum RepairStatus
{
    Received = 0,
    Diagnosing = 1,
    AwaitingApproval = 2,
    InRepair = 3,
    Ready = 4,
    Collected = 5,
    Cancelled = 6
}