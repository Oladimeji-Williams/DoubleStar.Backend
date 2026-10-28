using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Notifications.Application.Queries.DTOs;

namespace DoubleStar.Modules.Notifications.Application.Queries.GetNotificationLogQuery;

public sealed record GetNotificationLogQuery(
    PagedQuery Query)
    : IRequest<Result<PagedResult<NotificationLogDto>>>;