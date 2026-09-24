// Domain/Entities/NotificationLog.cs
using DoubleStar.SharedKernel.Domain;
using DoubleStar.Modules.Notifications.Domain.Enums;

namespace DoubleStar.Modules.Notifications.Domain.Entities;

public sealed class NotificationLog : Entity
{
    public NotificationChannel Channel { get; private set; }
    public string Recipient { get; private set; } = null!;
    public string TemplateName { get; private set; } = null!;
    public NotificationStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }

    private NotificationLog() { }

    public static NotificationLog Sent(NotificationChannel channel, string recipient, string templateName) => new()
    {
        Channel = channel,
        Recipient = recipient,
        TemplateName = templateName,
        Status = NotificationStatus.Sent,
    };

    public static NotificationLog Failed(
        NotificationChannel channel, string recipient, string templateName, string errorMessage) => new()
    {
        Channel = channel,
        Recipient = recipient,
        TemplateName = templateName,
        Status = NotificationStatus.Failed,
        ErrorMessage = errorMessage,
    };
}