using Microsoft.Extensions.Logging;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.Commands.CancelOrder;
using DoubleStar.Modules.Orders.Application.Services;
using DoubleStar.Modules.Orders.Domain.Entities;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Common.Primitives;
using DoubleStar.SharedKernel.Contracts.Inventory;
using DoubleStar.SharedKernel.Contracts.Orders;
using DoubleStar.SharedKernel.Contracts.Payments;
using MediatR;

namespace DoubleStar.Modules.Orders.Tests;

public sealed class CancelOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orders = new();
    private readonly Mock<IStockAdjuster> _stock = new();
    private readonly Mock<IPaymentVerifier> _paymentVerifier = new();
    private readonly Mock<IPublisher> _publisher = new();
    private readonly Guid _ownerId = Guid.NewGuid();

    public CancelOrderCommandHandlerTests()
    {
        _stock
            .Setup(s => s.ReleaseReservationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
    }

    private CancelOrderCommandHandler CreateHandler(Mock<ICurrentUser> user) => new(
        _orders.Object,
        user.Object,
        new OrderStockService(_stock.Object, Mock.Of<ILogger<OrderStockService>>()),
        _paymentVerifier.Object,
        _publisher.Object);

    private void GivenStoredOrder(Order order) =>
        _orders.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);

    private void VerifyNothingWasCancelled()
    {
        _stock.Verify(
            s => s.ReleaseReservationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _orders.Verify(r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisher.Verify(
            p => p.Publish(It.IsAny<OrderStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ForTheOwnerOfAnUnpaidOrder_CancelsReleasesStockAndPublishes()
    {
        var order = OrderTestSupport.PendingOrder(_ownerId);
        GivenStoredOrder(order);

        var result = await CreateHandler(OrderTestSupport.Customer(_ownerId))
            .Handle(new CancelOrderCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
        _stock.Verify(s => s.ReleaseReservationAsync(7, 2, It.IsAny<CancellationToken>()), Times.Once);
        _orders.Verify(r => r.SaveAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(
            p => p.Publish(
                It.Is<OrderStatusChangedEvent>(e => e.ToStatus == OrderStatus.Cancelled),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ForAnotherCustomersOrder_ReportsNotFoundAndChangesNothing()
    {
        var order = OrderTestSupport.PendingOrder(_ownerId);
        GivenStoredOrder(order);

        var result = await CreateHandler(OrderTestSupport.Customer(Guid.NewGuid()))
            .Handle(new CancelOrderCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "Orders.NotFound");
        order.Status.Should().Be(OrderStatus.PendingPayment);
        VerifyNothingWasCancelled();
    }

    [Fact]
    public async Task Handle_ForStaff_CancelsAnyCustomersUnpaidOrder()
    {
        var order = OrderTestSupport.PendingOrder(_ownerId);
        GivenStoredOrder(order);

        var result = await CreateHandler(OrderTestSupport.Staff(Guid.NewGuid()))
            .Handle(new CancelOrderCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_ForAnOrderThatIsAlreadyPaid_RefusesAndChangesNothing()
    {
        var order = OrderTestSupport.PaidOrder(_ownerId);
        GivenStoredOrder(order);

        var result = await CreateHandler(OrderTestSupport.Customer(_ownerId))
            .Handle(new CancelOrderCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Paid);
        VerifyNothingWasCancelled();
    }

    [Fact]
    public async Task Handle_WhenThePaymentJustCameThrough_DoesNotCancelThePaidOrder()
    {
        var order = OrderTestSupport.PendingOrder(_ownerId);
        order.AttachPayment("REF-9");
        GivenStoredOrder(order);
        _paymentVerifier
            .Setup(v => v.VerifyAsync("REF-9", It.IsAny<CancellationToken>()))
            .Callback(() => order.MarkPaid(Guid.NewGuid(), "REF-9", OrderTestSupport.Now))
            .ReturnsAsync(Result<PaymentSummaryDto>.Success(
                new PaymentSummaryDto(Guid.NewGuid(), PaymentSourceType.Order, 1, order.TotalKobo, "card", "Succeeded")));

        var result = await CreateHandler(OrderTestSupport.Customer(_ownerId))
            .Handle(new CancelOrderCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Paid);
        VerifyNothingWasCancelled();
    }

    [Fact]
    public async Task Handle_WhenNobodyIsSignedIn_FailsWithoutLoadingTheOrder()
    {
        var result = await CreateHandler(OrderTestSupport.Anonymous())
            .Handle(new CancelOrderCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        _orders.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}