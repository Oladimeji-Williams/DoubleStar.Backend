// Infrastructure/Email/ResendEmailSender.cs
using Microsoft.Extensions.Options;
using Resend;
using DoubleStar.SharedKernel.Abstractions.Notifications;
using DoubleStar.Modules.Notifications.Application.Templates;

namespace DoubleStar.Modules.Notifications.Infrastructure.Email;

internal sealed class ResendEmailSender(IResend resendClient, IOptions<EmailOptions> options) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var message = new EmailMessage
        {
            From = $"{_options.FromName} <{_options.FromAddress}>",
            Subject = subject,
            HtmlBody = htmlBody,
        };
        message.To.Add(toEmail);

        await resendClient.EmailSendAsync(message, cancellationToken);
    }

    public Task SendSignInCodeAsync(string toEmail, string code, string magicLink, CancellationToken cancellationToken)
    {
        var html = EmailTemplates.SignInCode(code, magicLink, DateTimeOffset.UtcNow);
        return SendAsync(toEmail, "Your Double Star sign-in code", html, cancellationToken);
    }

    public Task SendPasswordResetAsync(string toEmail, string resetLink, CancellationToken cancellationToken)
    {
        var html = EmailTemplates.PasswordReset(resetLink, DateTimeOffset.UtcNow);
        return SendAsync(toEmail, "Reset your Double Star password", html, cancellationToken);
    }

    public Task SendNotificationAsync(string toEmail, string title, string message, string? actionUrl, string? actionLabel, CancellationToken cancellationToken)
    {
        var html = EmailTemplates.Notification(title, message, actionUrl, actionLabel, DateTimeOffset.UtcNow);
        return SendAsync(toEmail, title, html, cancellationToken);
    }
    public Task SendEmailConfirmationAsync(string toEmail, string confirmLink, CancellationToken cancellationToken)
    {
        var html = EmailTemplates.EmailConfirmation(confirmLink, DateTimeOffset.UtcNow);
        return SendAsync(toEmail, "Confirm your Double Star email", html, cancellationToken);
    }
}