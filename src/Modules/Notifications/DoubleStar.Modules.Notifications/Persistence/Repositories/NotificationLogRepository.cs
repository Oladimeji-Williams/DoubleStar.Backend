// Persistence/Repositories/NotificationLogRepository.cs
using Microsoft.EntityFrameworkCore;
using DoubleStar.Modules.Notifications.Application.Abstractions;
using DoubleStar.Modules.Notifications.Domain.Entities;

namespace DoubleStar.Modules.Notifications.Persistence.Repositories;

public sealed class NotificationLogRepository(NotificationsDbContext dbContext) : INotificationLogRepository
{
    public async Task AddAsync(NotificationLog log, CancellationToken cancellationToken = default)
    {
        await dbContext.NotificationLogs.AddAsync(log, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationLog>> GetRecentAsync(int take, CancellationToken cancellationToken = default) =>
        await dbContext.NotificationLogs.OrderByDescending(n => n.CreatedAt).Take(take).ToListAsync(cancellationToken);
}