using DoubleStar.SharedKernel.Abstractions.Notifications;

namespace DoubleStar.Api.IntegrationTests.Fakes;

internal sealed class FakeEmailSender : IEmailSender
{
    public Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task SendSignInCodeAsync(
        string toEmail,
        string code,
        string magicLink,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(
        string toEmail,
        string resetLink,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task SendNotificationAsync(
        string toEmail,
        string title,
        string message,
        string? actionUrl,
        string? actionLabel,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task SendEmailConfirmationAsync(
        string toEmail,
        string confirmLink,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}