// CompleteSaleCommandHandlerTests.cs — the retry-safety behavior from Step 7 is the whole point of this file
using DoubleStar.SharedKernel.Common.Primitives;
using DoubleStar.SharedKernel.Contracts.Inventory;
using DoubleStar.Modules.Sales.Application.Abstractions;
using DoubleStar.Modules.Sales.Application.Commands.CompleteSaleCommand;
using DoubleStar.Modules.Sales.Domain.Entities;
using DoubleStar.Modules.Sales.Domain.Enums;

namespace DoubleStar.Modules.Sales.Tests;

public sealed class CompleteSaleCommandHandlerTests
{
    private readonly Mock<ISaleRepository> _saleRepository = new();
    private readonly Mock<IStockAdjuster> _stockAdjuster = new();

    private CompleteSaleCommandHandler CreateHandler() => new(_saleRepository.Object, _stockAdjuster.Object);

    [Fact]
    public async Task Handle_WhenAllDeductionsSucceed_CompletesTheSale()
    {
        var sale = Sale.CreateDraft(null);
        sale.AddLine(1, 1000, 2, null);
        _saleRepository.Setup(r => r.GetByIdAsync(sale.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sale);
        _stockAdjuster.Setup(a => a.DeductAsync(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());

        var result = await CreateHandler().Handle(new CompleteSaleCommand(sale.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(SaleStatus.Completed);
    }

    [Fact]
    public async Task Handle_WhenSecondLineFailsToDeduct_FirstLineStaysMarkedDeductedAndSaleStaysDraft()
    {
        var sale = Sale.CreateDraft(null);
        sale.AddLine(1, 1000, 1, null); // will succeed
        sale.AddLine(2, 2000, 1, null); // will fail

        _saleRepository.Setup(r => r.GetByIdAsync(sale.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sale);
        _stockAdjuster.Setup(a => a.DeductAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        _stockAdjuster.Setup(a => a.DeductAsync(2, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new Error("Inventory.InsufficientStock", "no stock", ErrorType.Conflict)));

        var result = await CreateHandler().Handle(new CompleteSaleCommand(sale.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        sale.Status.Should().Be(SaleStatus.Draft); // never completed
        sale.Lines.First(l => l.ProductId == 1).IsStockDeducted.Should().BeTrue(); // progress preserved
        sale.Lines.First(l => l.ProductId == 2).IsStockDeducted.Should().BeFalse();
        _saleRepository.Verify(r => r.UpdateAsync(sale, It.IsAny<CancellationToken>()), Times.Once); // progress was saved
    }

    [Fact]
    public async Task Handle_WhenCalledAgainAfterPartialFailure_DoesNotReDeductTheAlreadyDeductedLine()
    {
        var sale = Sale.CreateDraft(null);
        var line1 = sale.AddLine(1, 1000, 1, null);
        sale.AddLine(2, 2000, 1, null);
        sale.MarkLineDeducted(line1.Id); // simulates state left over from a prior partial attempt

        _saleRepository.Setup(r => r.GetByIdAsync(sale.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sale);
        _stockAdjuster.Setup(a => a.DeductAsync(2, 1, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());

        var result = await CreateHandler().Handle(new CompleteSaleCommand(sale.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _stockAdjuster.Verify(a => a.DeductAsync(1, It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _stockAdjuster.Verify(a => a.DeductAsync(2, 1, It.IsAny<CancellationToken>()), Times.Once);
    }
}