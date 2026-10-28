using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Notifications.Application.Abstractions;
using DoubleStar.Modules.Notifications.Application.Queries.DTOs;

namespace DoubleStar.Modules.Notifications.Application.Queries.GetNotificationLogQuery;

public sealed class GetNotificationLogQueryHandler(
    INotificationLogRepository notificationLogRepository)
    : IRequestHandler<
        GetNotificationLogQuery,
        Result<PagedResult<NotificationLogDto>>>
{
    public async Task<Result<PagedResult<NotificationLogDto>>> Handle(
        GetNotificationLogQuery request,
        CancellationToken cancellationToken)
    {
        var result =
            await notificationLogRepository.GetPagedAsync(
                request.Query,
                cancellationToken);

        var items =
            result.Items
                .Select(log =>
                    new NotificationLogDto(
                        log.Id,
                        log.Channel.ToString(),
                        log.Recipient,
                        log.TemplateName,
                        log.Status.ToString(),
                        log.ErrorMessage,
                        log.CreatedAt))
                .ToList();

        return Result<PagedResult<NotificationLogDto>>
            .Success(
                new PagedResult<NotificationLogDto>(
                    items,
                    result.Page,
                    result.PageSize,
                    result.TotalCount));
    }
}