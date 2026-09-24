using System.Net.Http.Headers;
using System.Net.Http.Json;
using DoubleStar.BuildingBlocks.Infrastructure.Api.Contracts;

namespace DoubleStar.Api.IntegrationTests;

internal static class ApiTestHelpers
{
    public static async Task<HttpClient> AuthenticatedAsAdminAsync(
        this HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/authentication/login",
            new
            {
                emailOrPhone = "admin@doublestar.local",
                password = "ChangeMe123!"
            });

        response.EnsureSuccessStatusCode();

        var body =
            await response.Content.ReadFromJsonAsync<
                ApiResponse<LoginResult>>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                body!.Data!.AccessToken);

        return client;
    }

    public sealed record LoginResult(string AccessToken);
}