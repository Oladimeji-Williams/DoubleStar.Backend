// SharedKernel/Contracts/Repairs/RepairStatusChangedEvent.cs
using MediatR;

namespace DoubleStar.SharedKernel.Contracts.Repairs;

public sealed record RepairStatusChangedEvent(
    int TicketId, Guid? CustomerId, RepairStatus FromStatus, RepairStatus ToStatus) : INotification;