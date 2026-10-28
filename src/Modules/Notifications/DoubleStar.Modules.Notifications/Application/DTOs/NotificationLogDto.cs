namespace DoubleStar.Modules.Notifications.Application.Queries.DTOs;

public sealed record NotificationLogDto(
    int Id,
    string Channel,
    string Recipient,
    string TemplateName,
    string Status,
    string? ErrorMessage,
    DateTime CreatedAt);