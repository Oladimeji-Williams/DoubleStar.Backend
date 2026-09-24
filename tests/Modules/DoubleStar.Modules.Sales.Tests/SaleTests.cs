// SaleTests.cs
using DoubleStar.Modules.Sales.Domain.Entities;

namespace DoubleStar.Modules.Sales.Tests;

public sealed class SaleTests
{
    [Fact]
    public void Complete_WithNoLines_Throws()
    {
        var sale = Sale.CreateDraft(null);
        var act = () => sale.Complete();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Complete_WithLines_TransitionsToCompleted()
    {
        var sale = Sale.CreateDraft(null);
        sale.AddLine(productId: 1, unitPriceKobo: 1000, quantity: 2, serialNumber: null);

        sale.Complete();

        sale.Status.Should().Be(Domain.Enums.SaleStatus.Completed);
        sale.TotalKobo.Should().Be(2000);
    }

    [Fact]
    public void AddLine_AfterCompleting_Throws()
    {
        var sale = Sale.CreateDraft(null);
        sale.AddLine(1, 1000, 1, null);
        sale.Complete();

        var act = () => sale.AddLine(2, 500, 1, null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Void_AfterCompleting_Throws()
    {
        var sale = Sale.CreateDraft(null);
        sale.AddLine(1, 1000, 1, null);
        sale.Complete();

        var act = () => sale.Void();

        act.Should().Throw<InvalidOperationException>();
    }
}