// Identity/Infrastructure/Turnstile/TurnstileVerifier.cs
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace DoubleStar.Modules.Identity.Infrastructure.Turnstile;

internal sealed class TurnstileVerifier(HttpClient httpClient, IOptions<TurnstileOptions> options) : ITurnstileVerifier
{
    private readonly TurnstileOptions _options = options.Value;

    public async Task<bool> VerifyAsync(string? token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;

        var form = new Dictionary<string, string> { ["secret"] = _options.SecretKey, ["response"] = token };
        var response = await httpClient.PostAsync(
            "https://challenges.cloudflare.com/turnstile/v0/siteverify", new FormUrlEncodedContent(form), cancellationToken);

        if (!response.IsSuccessStatusCode) return false;

        var payload = await response.Content.ReadFromJsonAsync<TurnstileResponse>(cancellationToken: cancellationToken);
        return payload?.Success ?? false;
    }

    private sealed record TurnstileResponse([property: JsonPropertyName("success")] bool Success);
}