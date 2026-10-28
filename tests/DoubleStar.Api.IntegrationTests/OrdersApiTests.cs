using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using DoubleStar.Api.IntegrationTests.Fakes;
using DoubleStar.BuildingBlocks.Infrastructure.Api.Contracts;

namespace DoubleStar.Api.IntegrationTests;

[Collection("Integration")]
public sealed class OrdersApiTests(DoubleStarApiFactory factory)
{
    [Theory]
    [InlineData("/api/v1/orders/mine")]
    [InlineData("/api/v1/orders")]
    [InlineData("/api/v1/orders/1")]
    public async Task Anonymous_requests_are_rejected(string url)
    {
        var response = await factory.CreateClient().GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Customers_cannot_use_the_staff_endpoints()
    {
        var customer = await factory.CreateCustomerClientAsync();

        (await customer.GetAsync("/api/v1/orders")).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var update = await customer.PutAsJsonAsync("/api/v1/orders/1/status", new { status = "Processing" });
        update.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task A_customer_cannot_see_or_change_another_customers_order()
    {
        var owner = await factory.CreateCustomerClientAsync();
        var stranger = await factory.CreateCustomerClientAsync();
        var placed = await PlaceOrderAsync(owner);
        var id = placed.Order.Id;

        (await stranger.GetAsync($"/api/v1/orders/{id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await stranger.PostAsync($"/api/v1/orders/{id}/cancel", null)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await stranger.PostAsync($"/api/v1/orders/{id}/retry-payment", null)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await owner.GetAsync($"/api/v1/orders/{id}")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task My_orders_lists_only_the_signed_in_customers_orders()
    {
        var owner = await factory.CreateCustomerClientAsync();
        var stranger = await factory.CreateCustomerClientAsync();
        var placed = await PlaceOrderAsync(owner);

        var ownerPage = await GetPageAsync(owner, "/api/v1/orders/mine");
        var strangerPage = await GetPageAsync(stranger, "/api/v1/orders/mine");

        ownerPage.Items.Should().ContainSingle(o => o.Id == placed.Order.Id);
        strangerPage.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Staff_can_list_and_open_any_order()
    {
        var customer = await factory.CreateCustomerClientAsync();
        var placed = await PlaceOrderAsync(customer);
        var staff = await factory.CreateClient().AuthenticatedAsAdminAsync();

        var page = await GetPageAsync(staff, "/api/v1/orders?pageSize=100");

        page.Items.Should().Contain(o => o.Id == placed.Order.Id);
        (await staff.GetAsync($"/api/v1/orders/{placed.Order.Id}")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Malformed_filters_are_rejected_with_a_400()
    {
        var customer = await factory.CreateCustomerClientAsync();

        var response = await customer.GetAsync("/api/v1/orders/mine?filters=not-json");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task A_customer_can_cancel_their_unpaid_order_once()
    {
        var customer = await factory.CreateCustomerClientAsync();
        var placed = await PlaceOrderAsync(customer);
        var id = placed.Order.Id;

        var first = await customer.PostAsync($"/api/v1/orders/{id}/cancel", null);
        var second = await customer.PostAsync($"/api/v1/orders/{id}/cancel", null);

        first.StatusCode.Should().Be(HttpStatusCode.NoContent);
        second.IsSuccessStatusCode.Should().BeFalse();
        (await GetOrderAsync(customer, id)).Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task A_paid_pickup_order_moves_through_fulfilment_in_order()
    {
        var customer = await factory.CreateCustomerClientAsync();
        var staff = await factory.CreateClient().AuthenticatedAsAdminAsync();
        var placed = await PlaceOrderAsync(customer);
        var id = placed.Order.Id;

        factory.Services.GetRequiredService<FakePaymentGateway>().MarkAsPaid(placed.PaymentReference);
        (await customer.PostAsJsonAsync($"/api/v1/checkout/{id}/verify", (object?)null)).EnsureSuccessStatusCode();
        (await GetOrderAsync(customer, id)).Status.Should().Be("Paid");

        var skip = await staff.PutAsJsonAsync($"/api/v1/orders/{id}/status", new { status = "Completed" });
        skip.IsSuccessStatusCode.Should().BeFalse("a paid order has to be processed first");

        foreach (var next in new[] { "Processing", "ReadyForPickup", "Completed" })
        {
            var step = await staff.PutAsJsonAsync($"/api/v1/orders/{id}/status", new { status = next });
            step.StatusCode.Should().Be(HttpStatusCode.NoContent, $"moving to {next} should be allowed");
        }

        (await GetOrderAsync(customer, id)).Status.Should().Be("Completed");

        var cancel = await customer.PostAsync($"/api/v1/orders/{id}/cancel", null);
        cancel.IsSuccessStatusCode.Should().BeFalse("a completed order can't be cancelled");
    }

    private async Task<PlacedOrderBody> PlaceOrderAsync(HttpClient customer)
    {
        var admin = await factory.CreateClient().AuthenticatedAsAdminAsync();
        var sku = $"SKU-{Guid.NewGuid():N}"[..12];

        var productResponse = await admin.PostAsJsonAsync(
            "/api/v1/products",
            new
            {
                name = "Test Charging Cable",
                sku,
                description = (string?)null,
                categoryId = (int?)null,
                brandId = (int?)null,
                unitPriceKobo = 1500_00,
                trackingMode = 0,
            });
        productResponse.EnsureSuccessStatusCode();
        var productId = (await productResponse.Content.ReadFromJsonAsync<ApiResponse<ProductBody>>())!.Data!.Id;

        (await admin.PostAsJsonAsync(
            "/api/v1/stock/restock", new { productId, quantity = 5, reference = (string?)null })).EnsureSuccessStatusCode();

        (await customer.PostAsJsonAsync("/api/v1/cart/items", new { productId, quantity = 1 })).EnsureSuccessStatusCode();

        var checkout = await customer.PostAsJsonAsync(
            "/api/v1/checkout",
            new
            {
                fulfillmentMethod = "Pickup",
                contactName = "Ada Obi",
                contactPhone = "+2348011112222",
                deliveryAddress = (string?)null,
                deliveryCity = (string?)null,
                deliveryState = (string?)null,
                notes = (string?)null,
                idempotencyKey = Guid.NewGuid().ToString(),
            });
        checkout.EnsureSuccessStatusCode();

        return (await checkout.Content.ReadFromJsonAsync<ApiResponse<PlacedOrderBody>>())!.Data!;
    }

    private static async Task<OrderBody> GetOrderAsync(HttpClient client, int id)
    {
        var response = await client.GetAsync($"/api/v1/orders/{id}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ApiResponse<OrderBody>>())!.Data!;
    }

    private static async Task<PageBody> GetPageAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ApiResponse<PageBody>>())!.Data!;
    }

    private sealed record ProductBody(int Id);
    private sealed record OrderBody(int Id, string Status);
    private sealed record PlacedOrderBody(OrderBody Order, string AuthorizationUrl, string PaymentReference);
    private sealed record PageBody(List<OrderBody> Items, int TotalCount);
}