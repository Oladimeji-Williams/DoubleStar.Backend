// ProductTests.cs
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.Modules.Catalog.Domain.Entities;

namespace DoubleStar.Modules.Catalog.Tests;

public sealed class ProductTests
{
    [Fact]
    public void Create_WithNegativePrice_Throws()
    {
        var act = () => Product.Create("Test", "SKU-1", null, null, null, -1, StockTrackingMode.Bulk);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetPrice_WithNegativeAmount_Throws()
    {
        var product = Product.Create("Test", "SKU-1", null, null, null, 1000, StockTrackingMode.Bulk);
        var act = () => product.SetPrice(-1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Archive_SetsIsArchivedTrue()
    {
        var product = Product.Create("Test", "SKU-1", null, null, null, 1000, StockTrackingMode.Bulk);
        product.Archive();
        product.IsArchived.Should().BeTrue();
    }

    [Fact]
    public void Create_UppercasesAndTrimsTheSku()
    {
        var product = Product.Create("Test", "  sku-1  ", null, null, null, 1000, StockTrackingMode.Bulk);
        product.Sku.Should().Be("SKU-1");
    }
}