// Abstractions/Notifications/IEmailSender.cs
namespace DoubleStar.SharedKernel.Abstractions.Notifications;

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken);
}