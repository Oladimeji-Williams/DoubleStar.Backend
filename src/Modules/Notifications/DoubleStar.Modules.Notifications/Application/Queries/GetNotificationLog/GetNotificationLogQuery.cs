// Application/Queries/GetNotificationLogQuery/GetNotificationLogQuery.cs
namespace DoubleStar.Modules.Notifications.Application.Queries.GetNotificationLogQuery;

public sealed record NotificationLogDto(
    int Id, string Channel, string Recipient, string TemplateName, string Status, string? ErrorMessage, DateTime CreatedAt);

public sealed record GetNotificationLogQuery(int Take = 100) : IRequest<Result<IReadOnlyList<NotificationLogDto>>>;