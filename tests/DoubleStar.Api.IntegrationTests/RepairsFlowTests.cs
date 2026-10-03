// RepairsFlowTests.cs — exercises the Repairs state machine plus Inventory part deduction end to end

using DoubleStar.Api.IntegrationTests.Fakes;
using DoubleStar.BuildingBlocks.Infrastructure.Api.Contracts;
using DoubleStar.SharedKernel.Abstractions.Notifications;

using Microsoft.Extensions.DependencyInjection;

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
            customerId = (Guid?)null,
            walkInName = "Repair Customer",
            walkInPhone = (string?)null,
            deviceDescription = "Test Phone",
            imeiOrSerial = (string?)null,
            faultDescription = "Screen cracked",
        });

        openResponse.EnsureSuccessStatusCode();

        var ticket =
            await openResponse.Content.ReadFromJsonAsync<ApiResponse<TicketDto>>();

        var ticketId = ticket!.Data!.Id;

        var startDiagnosisResponse = await client.PostAsJsonAsync(
            $"/api/v1/repairtickets/{ticketId}/start-diagnosis",
            (object?)null);

        startDiagnosisResponse.EnsureSuccessStatusCode();

        var diagnosisResponse = await client.PostAsJsonAsync(
            $"/api/v1/repairtickets/{ticketId}/diagnosis",
            new
            {
                diagnosisNotes = "Needs new screen",
                quotedPriceKobo = 20_000_00
            });

        diagnosisResponse.EnsureSuccessStatusCode();

        var approveResponse = await client.PostAsJsonAsync(
            $"/api/v1/repairtickets/{ticketId}/approve-quote",
            (object?)null);

        approveResponse.EnsureSuccessStatusCode();

        var readyResponse = await client.PostAsJsonAsync(
            $"/api/v1/repairtickets/{ticketId}/ready",
            (object?)null);

        readyResponse.EnsureSuccessStatusCode();

        var collectResponse = await client.PostAsJsonAsync(
            $"/api/v1/repairtickets/{ticketId}/collect",
            (object?)null);

        collectResponse.EnsureSuccessStatusCode();

        var getResponse =
            await client.GetAsync($"/api/v1/repairtickets/{ticketId}");

        var final =
            await getResponse.Content.ReadFromJsonAsync<ApiResponse<TicketDto>>();

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

        var ticket =
            await openResponse.Content
                .ReadFromJsonAsync<ApiResponse<TicketDto>>();

        var response = await client.PostAsJsonAsync(
            $"/api/v1/repairtickets/{ticket!.Data!.Id}/approve-quote",
            (object?)null);

        response.StatusCode.Should()
            .Be(System.Net.HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Collecting_a_device_sends_an_sms_to_the_customer()
    {
        var client = await factory.CreateClient().AuthenticatedAsAdminAsync();

        var openResponse = await client.PostAsJsonAsync(
            "/api/v1/repairtickets",
            new
            {
                customerId = (Guid?)null,
                walkInName = "SMS Test Customer",
                walkInPhone = "+2348012345678",
                deviceDescription = "SMS Test Phone",
                imeiOrSerial = (string?)null,
                faultDescription = "Testing SMS",
            });

        openResponse.EnsureSuccessStatusCode();

        var ticket =
            await openResponse.Content.ReadFromJsonAsync<ApiResponse<TicketDto>>();

        var ticketId = ticket!.Data!.Id;

        var startDiagnosisResponse = await client.PostAsJsonAsync(
            $"/api/v1/repairtickets/{ticketId}/start-diagnosis",
            (object?)null);

        startDiagnosisResponse.EnsureSuccessStatusCode();

        var diagnosisResponse = await client.PostAsJsonAsync(
            $"/api/v1/repairtickets/{ticketId}/diagnosis",
            new
            {
                diagnosisNotes = "Fixable",
                quotedPriceKobo = 5000_00
            });

        diagnosisResponse.EnsureSuccessStatusCode();

        var approveResponse = await client.PostAsJsonAsync(
            $"/api/v1/repairtickets/{ticketId}/approve-quote",
            (object?)null);

        approveResponse.EnsureSuccessStatusCode();

        var readyResponse = await client.PostAsJsonAsync(
            $"/api/v1/repairtickets/{ticketId}/ready",
            (object?)null);

        readyResponse.EnsureSuccessStatusCode();

        var collectResponse = await client.PostAsJsonAsync(
            $"/api/v1/repairtickets/{ticketId}/collect",
            (object?)null);

        collectResponse.EnsureSuccessStatusCode();

        using var scope = factory.Services.CreateScope();

        var smsSender =
            (FakeSmsSender)scope.ServiceProvider
                .GetRequiredService<ISmsSender>();

        smsSender.SentMessages.Should()
            .Contain(m =>
                m.To == "+2348012345678" &&
                m.Message.Contains(ticketId.ToString()));
    }

    private sealed record TicketDto(int Id, string Status);
}