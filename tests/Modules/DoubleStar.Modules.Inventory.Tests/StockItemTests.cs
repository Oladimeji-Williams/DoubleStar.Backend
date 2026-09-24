// StockItemTests.cs
using DoubleStar.Modules.Inventory.Domain.Entities;

namespace DoubleStar.Modules.Inventory.Tests;

public sealed class StockItemTests
{
    [Fact]
    public void Receive_IncreasesQuantityOnHand()
    {
        var item = StockItem.CreateEmpty(productId: 1);
        item.Receive(10);
        item.QuantityOnHand.Should().Be(10);
        item.QuantityAvailable.Should().Be(10);
    }

    [Fact]
    public void TryReserve_WhenInsufficientAvailable_ReturnsFalseAndDoesNotReserve()
    {
        var item = StockItem.CreateEmpty(productId: 1);
        item.Receive(5);

        var reserved = item.TryReserve(10);

        reserved.Should().BeFalse();
        item.QuantityReserved.Should().Be(0);
    }

    [Fact]
    public void TryReserve_WhenSufficient_ReservesAndReducesAvailable()
    {
        var item = StockItem.CreateEmpty(productId: 1);
        item.Receive(10);

        var reserved = item.TryReserve(4);

        reserved.Should().BeTrue();
        item.QuantityAvailable.Should().Be(6);
        item.QuantityOnHand.Should().Be(10);
    }

    [Fact]
    public void TryDeduct_ReducesBothOnHandAndAnyMatchingReservation()
    {
        var item = StockItem.CreateEmpty(productId: 1);
        item.Receive(10);
        item.TryReserve(4);

        var deducted = item.TryDeduct(4);

        deducted.Should().BeTrue();
        item.QuantityOnHand.Should().Be(6);
        item.QuantityReserved.Should().Be(0);
    }

    [Fact]
    public void SetOnHand_BelowCurrentlyReserved_Throws()
    {
        var item = StockItem.CreateEmpty(productId: 1);
        item.Receive(10);
        item.TryReserve(8);

        var act = () => item.SetOnHand(5);

        act.Should().Throw<InvalidOperationException>();
    }
}