// SharedKernel/Abstractions/Notifications/IEmailSender.cs — full replacement
namespace DoubleStar.SharedKernel.Abstractions.Notifications;

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken);
    Task SendSignInCodeAsync(string toEmail, string code, string magicLink, CancellationToken cancellationToken);
    Task SendPasswordResetAsync(string toEmail, string resetLink, CancellationToken cancellationToken);
    Task SendNotificationAsync(string toEmail, string title, string message, string? actionUrl, string? actionLabel, CancellationToken cancellationToken);
    Task SendEmailConfirmationAsync(string toEmail, string confirmLink, CancellationToken cancellationToken);
}