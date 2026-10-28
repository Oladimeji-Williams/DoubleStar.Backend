using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Notifications.Domain.Entities;

namespace DoubleStar.Modules.Notifications.Application.Abstractions;

public interface INotificationLogRepository
{
    Task AddAsync(
        NotificationLog log,
        CancellationToken cancellationToken = default);

    Task<PagedResult<NotificationLog>> GetPagedAsync(
        PagedQuery query,
        CancellationToken cancellationToken = default);
}