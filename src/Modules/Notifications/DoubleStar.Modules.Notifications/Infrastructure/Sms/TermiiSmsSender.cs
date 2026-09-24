// Infrastructure/Sms/TermiiSmsSender.cs
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using DoubleStar.SharedKernel.Abstractions.Notifications;

namespace DoubleStar.Modules.Notifications.Infrastructure.Sms;

internal sealed class TermiiSmsSender(HttpClient httpClient, IOptions<SmsOptions> options) : ISmsSender
{
    private readonly SmsOptions _options = options.Value;

    public async Task SendAsync(string toPhoneNumber, string message, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("api/sms/send", new
        {
            to = toPhoneNumber,
            from = _options.SenderId,
            sms = message,
            type = "plain",
            channel = "generic",
            api_key = _options.ApiKey,
        }, cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}