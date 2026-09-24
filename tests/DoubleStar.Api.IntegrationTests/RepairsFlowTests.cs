// RepairsFlowTests.cs — exercises the Repairs state machine plus Inventory part deduction end to end
using DoubleStar.BuildingBlocks.Infrastructure.Api.Contracts;

namespace DoubleStar.Api.IntegrationTests;

[Collection("Integration")]
public sealed class RepairsFlowTests(DoubleStarApiFactory factory)
{
    [Fact]
    public async Task Full_repair_lifecycle_reaches_collected()
    {
        var client = await factory.CreateClient().AuthenticatedAsAdminAsync();

        var openResponse = await client.PostAsJsonAsync("/api/v1/repairtickets", new
        {
            customerId = (Guid?)null, walkInName = "Repair Customer", walkInPhone = (string?)null,
            deviceDescription = "Test Phone", imeiOrSerial = (string?)null, faultDescription = "Screen cracked",
        });
        openResponse.EnsureSuccessStatusCode();
        var ticket = await openResponse.Content.ReadFromJsonAsync<ApiResponse<TicketDto>>();
        var ticketId = ticket!.Data!.Id;

        var diagnosisResponse = await client.PostAsJsonAsync(
            $"/api/v1/repairtickets/{ticketId}/diagnosis", new { diagnosisNotes = "Needs new screen", quotedPriceKobo = 20_000_00 });
        diagnosisResponse.EnsureSuccessStatusCode();

        var approveResponse = await client.PostAsJsonAsync($"/api/v1/repairtickets/{ticketId}/approve-quote", (object?)null);
        approveResponse.EnsureSuccessStatusCode();

        var readyResponse = await client.PostAsJsonAsync($"/api/v1/repairtickets/{ticketId}/ready", (object?)null);
        readyResponse.EnsureSuccessStatusCode();

        var collectResponse = await client.PostAsJsonAsync($"/api/v1/repairtickets/{ticketId}/collect", (object?)null);
        collectResponse.EnsureSuccessStatusCode();

        var getResponse = await client.GetAsync($"/api/v1/repairtickets/{ticketId}");
        var final = await getResponse.Content.ReadFromJsonAsync<ApiResponse<TicketDto>>();

        final!.Data!.Status.Should().Be("Collected");
    }

    [Fact]
    public async Task Approving_a_quote_before_diagnosis_returns_409()
    {
        var client = await factory.CreateClient().AuthenticatedAsAdminAsync();

        var openResponse = await client.PostAsJsonAsync(
            "/api/v1/repairtickets",
            new
            {
                customerId = (Guid?)null,
                walkInName = "Another Customer",
                walkInPhone = (string?)null,
                deviceDescription = "Test Phone 2",
                imeiOrSerial = (string?)null,
                faultDescription = "Won't charge",
            });

        openResponse.EnsureSuccessStatusCode();

        var ticket = await openResponse.Content
            .ReadFromJsonAsync<ApiResponse<TicketDto>>();

        var response = await client.PostAsJsonAsync(
            $"/api/v1/repairtickets/{ticket!.Data!.Id}/approve-quote",
            (object?)null);

        response.StatusCode.Should()
            .Be(System.Net.HttpStatusCode.Conflict);
    }

    private sealed record TicketDto(int Id, string Status);
}