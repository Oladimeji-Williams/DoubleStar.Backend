using DoubleStar.Modules.Orders.Domain.Entities;

namespace DoubleStar.Modules.Orders.Tests;

public sealed class CartTests
{
    private static Cart NewCart() => Cart.CreateFor(Guid.NewGuid());

    [Fact]
    public void AddItem_SameProductTwice_MergesIntoOneLine()
    {
        var cart = NewCart();
        cart.AddItem(1, 2);
        cart.AddItem(1, 3);

        cart.Items.Should().ContainSingle().Which.Quantity.Should().Be(5);
    }

    [Fact]
    public void AddItem_ClampsToMaxQuantityPerLine()
    {
        var cart = NewCart();
        cart.AddItem(1, Cart.MaxQuantityPerLine);
        cart.AddItem(1, 5);

        cart.Items.Single().Quantity.Should().Be(Cart.MaxQuantityPerLine);
    }

    [Fact]
    public void AddItem_WithZeroQuantity_Throws()
    {
        var act = () => NewCart().AddItem(1, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetQuantity_ToZero_RemovesTheLine()
    {
        var cart = NewCart();
        cart.AddItem(1, 2);

        cart.SetQuantity(1, 0);

        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public void AddItem_BeyondDistinctItemLimit_Throws()
    {
        var cart = NewCart();
        for (var i = 1; i <= Cart.MaxDistinctItems; i++)
        {
            cart.AddItem(i, 1);
        }

        var act = () => cart.AddItem(9999, 1);

        act.Should().Throw<InvalidOperationException>();
    }
}