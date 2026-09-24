// NotificationDispatcherTests.cs
using DoubleStar.SharedKernel.Abstractions.Notifications;
using DoubleStar.Modules.Notifications.Application.Abstractions;
using DoubleStar.Modules.Notifications.Application.Services;
using DoubleStar.Modules.Notifications.Domain.Entities;
using DoubleStar.Modules.Notifications.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace DoubleStar.Modules.Notifications.Tests;

public sealed class NotificationDispatcherTests
{
    private readonly Mock<IEmailSender> _emailSender = new();
    private readonly Mock<ISmsSender> _smsSender = new();
    private readonly Mock<INotificationLogRepository> _logRepository = new();

    private NotificationDispatcher CreateDispatcher() =>
        new(_emailSender.Object, _smsSender.Object, _logRepository.Object, Mock.Of<ILogger<NotificationDispatcher>>());

    [Fact]
    public async Task SendEmailAsync_WhenSenderSucceeds_LogsSent()
    {
        await CreateDispatcher().SendEmailAsync("ada@example.com", "Subject", "<p>Body</p>", "Template", CancellationToken.None);

        _logRepository.Verify(r => r.AddAsync(
            It.Is<NotificationLog>(l => l.Status == NotificationStatus.Sent && l.Recipient == "ada@example.com"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WhenSenderThrows_LogsFailedInsteadOfPropagatingTheException()
    {
        _emailSender.Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Resend is down"));

        var act = async () => await CreateDispatcher().SendEmailAsync("ada@example.com", "Subject", "<p>Body</p>", "Template", CancellationToken.None);

        await act.Should().NotThrowAsync();
        _logRepository.Verify(r => r.AddAsync(
            It.Is<NotificationLog>(l => l.Status == NotificationStatus.Failed), It.IsAny<CancellationToken>()), Times.Once);
    }
}