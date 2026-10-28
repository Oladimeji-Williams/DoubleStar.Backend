
using DoubleStar.SharedKernel.Contracts.Repairs;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace DoubleStar.Api.Realtime;

public sealed record RepairStatusUpdate(
    int TicketId,
    string FromStatus,
    string ToStatus,
    DateTimeOffset ChangedAtUtc);

public sealed class RepairStatusChangedSignalRHandler(
    IHubContext<RepairTrackingHub> hubContext)
    : INotificationHandler<RepairStatusChangedEvent>
{
    public async Task Handle(
        RepairStatusChangedEvent notification,
        CancellationToken cancellationToken)
    {
        var update = new RepairStatusUpdate(
            notification.TicketId,
            notification.FromStatus.ToString(),
            notification.ToStatus.ToString(),
            DateTimeOffset.UtcNow);

        await hubContext.Clients
            .Group(RepairTrackingHub.GroupName(notification.TicketId))
            .SendAsync(
                "RepairStatusChanged",
                update,
                cancellationToken);
    }
}
