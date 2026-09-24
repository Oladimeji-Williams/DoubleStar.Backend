// StockAdjusterServiceTests.cs
using DoubleStar.Modules.Inventory.Application.Abstractions;
using DoubleStar.Modules.Inventory.Application.CrossModule;
using DoubleStar.Modules.Inventory.Domain.Entities;

namespace DoubleStar.Modules.Inventory.Tests;

public sealed class StockAdjusterServiceTests
{
    private readonly Mock<IStockItemRepository> _stockItems = new();
    private readonly Mock<ISerializedUnitRepository> _serializedUnits = new();
    private readonly Mock<IStockMovementRepository> _movements = new();

    private StockAdjusterService CreateService() => new(_stockItems.Object, _serializedUnits.Object, _movements.Object);

    [Fact]
    public async Task DeductAsync_WhenProductHasNoStockRecord_ReturnsNotFound()
    {
        _stockItems.Setup(r => r.GetByProductIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((StockItem?)null);

        var result = await CreateService().DeductAsync(1, 1, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Inventory.ProductNotTracked");
    }

    [Fact]
    public async Task DeductAsync_WhenNotEnoughStock_ReturnsInsufficientStock()
    {
        var item = StockItem.CreateEmpty(1);
        item.Receive(2);
        _stockItems.Setup(r => r.GetByProductIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(item);

        var result = await CreateService().DeductAsync(1, 5, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Inventory.InsufficientStock");
    }

    [Fact]
    public async Task DeductSerializedAsync_WhenAlreadySold_ReturnsConflict()
    {
        var unit = SerializedUnit.Receive(1, "IMEI123");
        unit.TryMarkSold();
        _serializedUnits.Setup(r => r.GetBySerialAsync("IMEI123", It.IsAny<CancellationToken>())).ReturnsAsync(unit);

        var result = await CreateService().DeductSerializedAsync("IMEI123", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Inventory.SerialAlreadySold");
    }
}