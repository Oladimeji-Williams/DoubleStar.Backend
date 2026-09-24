// Application/Services/NotificationDispatcher.cs
using Microsoft.Extensions.Logging;
using DoubleStar.SharedKernel.Abstractions.Notifications;
using DoubleStar.Modules.Notifications.Application.Abstractions;
using DoubleStar.Modules.Notifications.Domain.Entities;
using DoubleStar.Modules.Notifications.Domain.Enums;

namespace DoubleStar.Modules.Notifications.Application.Services;

/// <summary>
/// A failed send is logged, never thrown — the caller (an event handler
/// reacting to a repair status change, say) shouldn't fail its own operation
/// just because an SMS provider had a bad moment.
/// </summary>
public sealed class NotificationDispatcher(
    IEmailSender emailSender, ISmsSender smsSender,
    INotificationLogRepository notificationLogRepository, ILogger<NotificationDispatcher> logger)
{
    public async Task SendEmailAsync(
        string recipient, string subject, string htmlBody, string templateName, CancellationToken cancellationToken)
    {
        try
        {
            await emailSender.SendAsync(recipient, subject, htmlBody, cancellationToken);
            await notificationLogRepository.AddAsync(
                NotificationLog.Sent(NotificationChannel.Email, recipient, templateName), cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to email {Recipient} using {Template}", recipient, templateName);
            await notificationLogRepository.AddAsync(
                NotificationLog.Failed(NotificationChannel.Email, recipient, templateName, ex.Message), cancellationToken);
        }
    }

    public async Task SendSmsAsync(string recipient, string message, string templateName, CancellationToken cancellationToken)
    {
        try
        {
            await smsSender.SendAsync(recipient, message, cancellationToken);
            await notificationLogRepository.AddAsync(
                NotificationLog.Sent(NotificationChannel.Sms, recipient, templateName), cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to SMS {Recipient} using {Template}", recipient, templateName);
            await notificationLogRepository.AddAsync(
                NotificationLog.Failed(NotificationChannel.Sms, recipient, templateName, ex.Message), cancellationToken);
        }
    }
}