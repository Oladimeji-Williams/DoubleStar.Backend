using DoubleStar.Modules.Sales.Application.Abstractions;
using DoubleStar.Modules.Sales.Application.Commands.CompleteSaleCommand;
using DoubleStar.Modules.Sales.Domain.Entities;
using DoubleStar.Modules.Sales.Domain.Enums;
using DoubleStar.SharedKernel.Common.Primitives;
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.SharedKernel.Contracts.Inventory;

namespace DoubleStar.Modules.Sales.Tests;

public sealed class CompleteSaleCommandHandlerTests
{
    private readonly Mock<ISaleRepository> _saleRepository = new();
    private readonly Mock<IStockAdjuster> _stockAdjuster = new();
    private readonly Mock<IProductCatalog> _productCatalog = new();

    public CompleteSaleCommandHandlerTests()
    {
        // CompleteSaleCommandHandler now enriches the returned SaleDto
        // with product names. Returning an empty collection is sufficient
        // for these retry-safety tests because the enricher has a fallback
        // name for products that are not found in the catalog.
        _productCatalog
            .Setup(c => c.GetByIdsAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
    }

    private CompleteSaleCommandHandler CreateHandler() =>
        new(
            _saleRepository.Object,
            _stockAdjuster.Object,
            _productCatalog.Object);

    [Fact]
    public async Task Handle_WhenAllDeductionsSucceed_CompletesTheSale()
    {
        // Arrange
        var sale = Sale.CreateDraft(null);
        sale.AddLine(1, 1000, 2, null);

        _saleRepository
            .Setup(r => r.GetByIdAsync(
                sale.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        _stockAdjuster
            .Setup(a => a.DeductAsync(
                1,
                2,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await CreateHandler()
            .Handle(
                new CompleteSaleCommand(sale.Id),
                CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(SaleStatus.Completed);
    }

    [Fact]
    public async Task Handle_WhenSecondLineFailsToDeduct_FirstLineStaysMarkedDeductedAndSaleStaysDraft()
    {
        // Arrange
        var sale = Sale.CreateDraft(null);

        sale.AddLine(1, 1000, 1, null); // will succeed
        sale.AddLine(2, 2000, 1, null); // will fail

        _saleRepository
            .Setup(r => r.GetByIdAsync(
                sale.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        _stockAdjuster
            .Setup(a => a.DeductAsync(
                1,
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _stockAdjuster
            .Setup(a => a.DeductAsync(
                2,
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Failure(
                    new Error(
                        "Inventory.InsufficientStock",
                        "no stock",
                        ErrorType.Conflict)));

        // Act
        var result = await CreateHandler()
            .Handle(
                new CompleteSaleCommand(sale.Id),
                CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        sale.Status.Should().Be(SaleStatus.Draft);

        sale.Lines
            .First(l => l.ProductId == 1)
            .IsStockDeducted
            .Should()
            .BeTrue();

        sale.Lines
            .First(l => l.ProductId == 2)
            .IsStockDeducted
            .Should()
            .BeFalse();

        _saleRepository.Verify(
            r => r.UpdateAsync(
                sale,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCalledAgainAfterPartialFailure_DoesNotReDeductTheAlreadyDeductedLine()
    {
        // Arrange
        var sale = Sale.CreateDraft(null);

        var line1 = sale.AddLine(1, 1000, 1, null);
        sale.AddLine(2, 2000, 1, null);

        // Simulate state left over from a previous partial attempt.
        sale.MarkLineDeducted(line1.Id);

        _saleRepository
            .Setup(r => r.GetByIdAsync(
                sale.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        _stockAdjuster
            .Setup(a => a.DeductAsync(
                2,
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await CreateHandler()
            .Handle(
                new CompleteSaleCommand(sale.Id),
                CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _stockAdjuster.Verify(
            a => a.DeductAsync(
                1,
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _stockAdjuster.Verify(
            a => a.DeductAsync(
                2,
                1,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}