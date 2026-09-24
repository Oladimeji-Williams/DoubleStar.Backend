// .../GetNotificationLogQueryHandler.cs
using DoubleStar.Modules.Notifications.Application.Abstractions;

namespace DoubleStar.Modules.Notifications.Application.Queries.GetNotificationLogQuery;

public sealed class GetNotificationLogQueryHandler(INotificationLogRepository notificationLogRepository)
    : IRequestHandler<GetNotificationLogQuery, Result<IReadOnlyList<NotificationLogDto>>>
{
    public async Task<Result<IReadOnlyList<NotificationLogDto>>> Handle(
        GetNotificationLogQuery request, CancellationToken cancellationToken)
    {
        var logs = await notificationLogRepository.GetRecentAsync(request.Take, cancellationToken);
        return Result<IReadOnlyList<NotificationLogDto>>.Success(
            logs.Select(l => new NotificationLogDto(
                l.Id, l.Channel.ToString(), l.Recipient, l.TemplateName, l.Status.ToString(), l.ErrorMessage, l.CreatedAt))
            .ToList());
    }
}