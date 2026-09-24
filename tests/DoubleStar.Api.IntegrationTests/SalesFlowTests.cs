// SalesFlowTests.cs — exercises Catalog + Inventory + Sales together, matching the Step 7 smoke test
using DoubleStar.BuildingBlocks.Infrastructure.Api.Contracts;

namespace DoubleStar.Api.IntegrationTests;

[Collection("Integration")]
public sealed class SalesFlowTests(DoubleStarApiFactory factory)
{
    [Fact]
    public async Task Completing_a_sale_deducts_stock_and_marks_the_sale_completed()
    {
        var client = await factory.CreateClient().AuthenticatedAsAdminAsync();
        var sku = $"SKU-{Guid.NewGuid():N}"[..12];

        var productResponse = await client.PostAsJsonAsync(
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
        var product = await productResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        var productId = product!.Data!.Id;

        await client.PostAsJsonAsync("/api/v1/stock/restock", new { productId, quantity = 10, reference = (string?)null });

        var saleResponse = await client.PostAsJsonAsync("/api/v1/sales", new
        {
            customerId = (Guid?)null, walkInName = "Test Walk-In", walkInPhone = (string?)null,
        });
        saleResponse.EnsureSuccessStatusCode();
        var sale = await saleResponse.Content.ReadFromJsonAsync<ApiResponse<SaleDto>>();
        var saleId = sale!.Data!.Id;

        await client.PostAsJsonAsync($"/api/v1/sales/{saleId}/lines", new { productId, quantity = 3, serialNumber = (string?)null });

        var completeResponse = await client.PostAsJsonAsync($"/api/v1/sales/{saleId}/complete", (object?)null);
        completeResponse.EnsureSuccessStatusCode();
        var completed = await completeResponse.Content.ReadFromJsonAsync<ApiResponse<SaleDto>>();

        completed!.Data!.Status.Should().Be("Completed");

        var stockResponse = await client.GetAsync($"/api/v1/stock/{productId}");
        var stock = await stockResponse.Content.ReadFromJsonAsync<ApiResponse<StockLevelDto>>();
        stock!.Data!.AvailableQuantity.Should().Be(7); // 10 restocked - 3 sold
    }

    private sealed record ProductDto(int Id);
    private sealed record SaleDto(int Id, string Status);
    private sealed record StockLevelDto(int ProductId, int AvailableQuantity, int ReservedQuantity);
}