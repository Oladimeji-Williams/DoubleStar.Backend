using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using DoubleStar.Modules.Identity.Application.Abstractions;

namespace DoubleStar.Modules.Identity.Infrastructure.Turnstile;

internal sealed class TurnstileVerifier(
    HttpClient httpClient,
    IOptions<TurnstileOptions> options,
    IHostEnvironment environment) : IHumanVerifier
{
    private const string VerifyUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";
    private readonly TurnstileOptions _options = options.Value;

    public async Task<bool> VerifyAsync(string? token, CancellationToken cancellationToken)
    {
        // Integration tests have no browser, so there is no widget to solve.
        if (environment.IsEnvironment("Testing")) return true;

        if (string.IsNullOrWhiteSpace(token)) return false;

        var form = new Dictionary<string, string> { ["secret"] = _options.SecretKey, ["response"] = token };
        var response = await httpClient.PostAsync(VerifyUrl, new FormUrlEncodedContent(form), cancellationToken);

        if (!response.IsSuccessStatusCode) return false;

        var payload = await response.Content.ReadFromJsonAsync<TurnstileResponse>(cancellationToken: cancellationToken);
        return payload?.Success ?? false;
    }

    private sealed record TurnstileResponse([property: JsonPropertyName("success")] bool Success);
}