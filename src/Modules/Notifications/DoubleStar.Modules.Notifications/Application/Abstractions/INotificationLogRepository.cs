// Application/Abstractions/INotificationLogRepository.cs
using DoubleStar.Modules.Notifications.Domain.Entities;

namespace DoubleStar.Modules.Notifications.Application.Abstractions;

public interface INotificationLogRepository
{
    Task AddAsync(NotificationLog log, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationLog>> GetRecentAsync(int take, CancellationToken cancellationToken = default);
}