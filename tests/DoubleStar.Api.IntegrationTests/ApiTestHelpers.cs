// tests/DoubleStar.Api.IntegrationTests/ApiTestHelpers.cs — full replacement
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DoubleStar.BuildingBlocks.Infrastructure.Api.Contracts;

namespace DoubleStar.Api.IntegrationTests;

internal static class ApiTestHelpers
{
    public static async Task<HttpClient> AuthenticatedAsAdminAsync(this HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/authentication/login",
            new { emailOrPhone = "admin@doublestar.local", password = "ChangeMe123!" });

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Admin login failed.\nStatus: {(int)response.StatusCode} {response.StatusCode}\nResponse: {responseBody}");
        }

        var body = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<LoginResponseBody>>(
            responseBody,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var accessToken = body?.Data?.AuthResult?.AccessToken;
        if (accessToken is null)
        {
            throw new Exception($"Admin login succeeded but no access token was returned.\nResponse: {responseBody}");
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    public sealed record LoginResponseBody(bool RequiresTwoFactor, string? ChallengeToken, AuthResultBody? AuthResult);
    public sealed record AuthResultBody(string AccessToken);
}