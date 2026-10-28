using System.Net;

namespace DoubleStar.Api.IntegrationTests;

[Collection("Integration")]
public sealed class HealthApiTests(DoubleStarApiFactory factory)
{
    [Theory]
    [InlineData("/health")]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task Health_endpoints_report_healthy(string url)
    {
        var response = await factory.CreateClient().GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}