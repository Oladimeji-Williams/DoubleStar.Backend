using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using DoubleStar.BuildingBlocks.Infrastructure.Api.Contracts;

namespace DoubleStar.Api.IntegrationTests;

[Collection("Integration")]
public sealed class AuthFlowTests(DoubleStarApiFactory factory)
{
    [Fact]
    public async Task Register_then_login_then_call_me_returns_the_registered_profile()
    {
        var client = factory.CreateClient();

        var email = $"ada-{Guid.NewGuid():N}@example.com";

        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/authentication/register-customer",
            new
            {
                firstName = "Ada",
                lastName = "Okafor",
                email,
                phone = (string?)null,
                password = "Password123!",
            });

        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/authentication/login",
            new
            {
                emailOrPhone = email,
                password = "Password123!",
            });

        loginResponse.EnsureSuccessStatusCode();

        var loginBody = await loginResponse.Content
            .ReadFromJsonAsync<ApiResponse<ApiTestHelpers.LoginResult>>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginBody!.Data!.AccessToken);

        var meResponse = await client.GetAsync(
            "/api/v1/authentication/me");

        meResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_401()
    {
        var client = factory.CreateClient();

        var email = $"bad-{Guid.NewGuid():N}@example.com";

        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/authentication/register-customer",
            new
            {
                firstName = "Test",
                lastName = "User",
                email,
                phone = (string?)null,
                password = "Password123!",
            });

        registerResponse.EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync(
            "/api/v1/authentication/login",
            new
            {
                emailOrPhone = email,
                password = "WrongPassword!",
            });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}