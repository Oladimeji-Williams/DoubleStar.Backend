// CreateProductCommandHandlerTests.cs
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.Modules.Catalog.Application.Abstractions;
using DoubleStar.Modules.Catalog.Application.Commands.CreateProductCommand;
using DoubleStar.Modules.Catalog.Domain.Entities;

namespace DoubleStar.Modules.Catalog.Tests;

public sealed class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _repository = new();

    [Fact]
    public async Task Handle_WhenSkuAlreadyExists_ReturnsConflict()
    {
        _repository.Setup(r => r.GetBySkuAsync("SKU-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Product.Create("Existing", "SKU-1", null, null, null, 1000, StockTrackingMode.Bulk));

        var handler = new CreateProductCommandHandler(_repository.Object);
        var command = new CreateProductCommand("New", "SKU-1", null, null, null, 2000, StockTrackingMode.Bulk);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Catalog.SkuAlreadyExists");
    }

    [Fact]
    public async Task Handle_WhenSkuIsNew_CreatesTheProduct()
    {
        _repository.Setup(r => r.GetBySkuAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var handler = new CreateProductCommandHandler(_repository.Object);
        var command = new CreateProductCommand("New", "SKU-2", null, null, null, 2000, StockTrackingMode.Serialized);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TrackingMode.Should().Be(StockTrackingMode.Serialized);
    }
}