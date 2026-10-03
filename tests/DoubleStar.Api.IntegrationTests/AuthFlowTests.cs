// tests/DoubleStar.Api.IntegrationTests/AuthFlowTests.cs — full replacement
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using DoubleStar.BuildingBlocks.Infrastructure.Api.Contracts;
using DoubleStar.SharedKernel.Abstractions.Authentication;

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
            new { email, phone = (string?)null, password = "Password123!" });
        registerResponse.EnsureSuccessStatusCode();

        await ConfirmEmailAsync(email);

        var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/authentication/login",
            new { emailOrPhone = email, password = "Password123!" });
        loginResponse.EnsureSuccessStatusCode();

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<ApiTestHelpers.LoginResponseBody>>();
        loginBody!.Data!.RequiresTwoFactor.Should().BeFalse();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody.Data.AuthResult!.AccessToken);

        var meResponse = await client.GetAsync("/api/v1/authentication/me");
        meResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_401()
    {
        var client = factory.CreateClient();
        var email = $"bad-{Guid.NewGuid():N}@example.com";

        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/authentication/register-customer",
            new { email, phone = (string?)null, password = "Password123!" });
        registerResponse.EnsureSuccessStatusCode();

        await ConfirmEmailAsync(email);

        var response = await client.PostAsJsonAsync(
            "/api/v1/authentication/login",
            new { emailOrPhone = email, password = "WrongPassword!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task ConfirmEmailAsync(string email)
    {
        using var scope = factory.Services.CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
        var token = await identityService.GenerateEmailConfirmationTokenAsync(email, CancellationToken.None);

        var confirmResponse = await factory.CreateClient().PostAsJsonAsync(
            "/api/v1/authentication/confirm-email", new { email, token });
        confirmResponse.EnsureSuccessStatusCode();
    }
}