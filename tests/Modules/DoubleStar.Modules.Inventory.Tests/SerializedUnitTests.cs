// SerializedUnitTests.cs
using DoubleStar.Modules.Inventory.Domain.Entities;
using DoubleStar.Modules.Inventory.Domain.Enums;

namespace DoubleStar.Modules.Inventory.Tests;

public sealed class SerializedUnitTests
{
    [Fact]
    public void TryMarkSold_WhenInStock_SucceedsAndUpdatesStatus()
    {
        var unit = SerializedUnit.Receive(productId: 1, "IMEI123");
        var sold = unit.TryMarkSold();

        sold.Should().BeTrue();
        unit.Status.Should().Be(SerializedUnitStatus.Sold);
    }

    [Fact]
    public void TryMarkSold_WhenAlreadySold_ReturnsFalse()
    {
        var unit = SerializedUnit.Receive(productId: 1, "IMEI123");
        unit.TryMarkSold();

        var soldAgain = unit.TryMarkSold();

        soldAgain.Should().BeFalse();
    }
}