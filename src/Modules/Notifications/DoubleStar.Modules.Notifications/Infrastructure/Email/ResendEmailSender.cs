// Infrastructure/Email/ResendEmailSender.cs
using Microsoft.Extensions.Options;
using Resend;
using DoubleStar.SharedKernel.Abstractions.Notifications;

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
}