using System.Net.Http.Headers;
using System.Net.Http.Json;
using DoubleStar.BuildingBlocks.Infrastructure.Api.Contracts;
using Microsoft.Extensions.DependencyInjection;
using DoubleStar.SharedKernel.Abstractions.Authentication;


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

    /// <summary>Registers a brand-new customer, confirms their email and returns a client signed in as them.</summary>
    public static async Task<HttpClient> CreateCustomerClientAsync(this DoubleStarApiFactory factory)
    {
        var client = factory.CreateClient();
        var email = $"customer-{Guid.NewGuid():N}@example.com";

        (await client.PostAsJsonAsync(
            "/api/v1/authentication/register-customer",
            new { email, phone = (string?)null, password = "Password123!" })).EnsureSuccessStatusCode();

        using (var scope = factory.Services.CreateScope())
        {
            var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
            var token = await identityService.GenerateEmailConfirmationTokenAsync(email, CancellationToken.None);
            (await factory.CreateClient().PostAsJsonAsync(
                "/api/v1/authentication/confirm-email", new { email, token })).EnsureSuccessStatusCode();
        }

        var login = await client.PostAsJsonAsync(
            "/api/v1/authentication/login", new { emailOrPhone = email, password = "Password123!" });
        login.EnsureSuccessStatusCode();

        var body = await login.Content.ReadFromJsonAsync<ApiResponse<LoginResponseBody>>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", body!.Data!.AuthResult!.AccessToken);
        return client;
    }

    public sealed record LoginResponseBody(bool RequiresTwoFactor, string? ChallengeToken, AuthResultBody? AuthResult);
    public sealed record AuthResultBody(string AccessToken);
}