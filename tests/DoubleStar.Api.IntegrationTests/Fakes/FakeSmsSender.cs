// tests/DoubleStar.Api.IntegrationTests/Fakes/FakeSmsSender.cs
using System.Collections.Concurrent;
using DoubleStar.SharedKernel.Abstractions.Notifications;

namespace DoubleStar.Api.IntegrationTests.Fakes;

/// <summary>Records every SMS sent during a test run so assertions can inspect them —
/// mirrors FakeEmailSender, but keeps a queryable log instead of discarding silently,
/// since SMS content (e.g. repair-status text) is worth asserting on directly.</summary>
internal sealed class FakeSmsSender : ISmsSender
{
    public ConcurrentBag<(string To, string Message)> SentMessages { get; } = [];

    public Task SendAsync(string toPhoneNumber, string message, CancellationToken cancellationToken)
    {
        SentMessages.Add((toPhoneNumber, message));
        return Task.CompletedTask;
    }
}