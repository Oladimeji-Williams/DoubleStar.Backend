using Microsoft.Extensions.Logging;
using DoubleStar.SharedKernel.Common.Primitives;
using DoubleStar.SharedKernel.Contracts.Inventory;
using DoubleStar.SharedKernel.Contracts.Orders;
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.EventHandlers;
using DoubleStar.Modules.Orders.Application.Services;
using DoubleStar.Modules.Orders.Domain.Entities;
using MediatR;

namespace DoubleStar.Modules.Orders.Tests;

public sealed class PaymentSucceededEventHandlerTests
{
    private readonly Mock<IOrderRepository> _orders = new();
    private readonly Mock<IStockAdjuster> _stock = new();
    private readonly Mock<IPublisher> _publisher = new();

    private PaymentSucceededEventHandler CreateHandler() => new(
        _orders.Object,
        new OrderStockService(_stock.Object, Mock.Of<ILogger<OrderStockService>>()),
        _publisher.Object,
        TimeProvider.System,
        Mock.Of<ILogger<PaymentSucceededEventHandler>>());

    private static Order PendingOrder() => Order.Place(
        Guid.NewGuid(), FulfillmentMethod.Pickup, "Ada", "+2348011112222",
        null, null, null, null, null, 0, DateTime.UtcNow.AddMinutes(30),
        [new OrderLineDraft(7, "Cable", "CBL", 1_500_00, 2, false, [])]);

    private static PaymentSucceededEvent EventFor(Order order, int orderId = 1) =>
        new(Guid.NewGuid(), PaymentSourceType.Order, orderId, order.TotalKobo, "REF-1");

    [Fact]
    public async Task Handle_ForANonOrderPayment_DoesNothing()
    {
        var evt = new PaymentSucceededEvent(Guid.NewGuid(), PaymentSourceType.Sale, 1, 100, "REF");

        await CreateHandler().Handle(evt, CancellationToken.None);

        _orders.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ForAPendingOrder_ConsumesStock_MarksPaid_AndPublishes()
    {
        var order = PendingOrder();
        _orders.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _stock.Setup(s => s.ConsumeReservationAsync(7, 2, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());

        await CreateHandler().Handle(EventFor(order), CancellationToken.None);

        order.Status.Should().Be(OrderStatus.Paid);
        order.NeedsAttention.Should().BeFalse();
        _stock.Verify(s => s.ConsumeReservationAsync(7, 2, It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(
            p => p.Publish(
                It.Is<OrderStatusChangedEvent>(e => e.ToStatus == OrderStatus.Paid), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRedelivered_IsIdempotent()
    {
        var order = PendingOrder();
        order.MarkPaid(Guid.NewGuid(), "REF-1", DateTime.UtcNow);
        _orders.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        await CreateHandler().Handle(EventFor(order), CancellationToken.None);

        _stock.Verify(
            s => s.ConsumeReservationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisher.Verify(p => p.Publish(It.IsAny<OrderStatusChangedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PaymentAfterCancellation_FlagsTheOrderForAttention()
    {
        var order = PendingOrder();
        order.Cancel("customer cancelled");
        _orders.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        await CreateHandler().Handle(EventFor(order), CancellationToken.None);

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.NeedsAttention.Should().BeTrue();
        _stock.Verify(
            s => s.ConsumeReservationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenStockConsumptionFails_StillMarksPaid_ButFlagsIt()
    {
        var order = PendingOrder();
        _orders.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _stock.Setup(s => s.ConsumeReservationAsync(7, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new Error("Inventory.InsufficientStock", "no", ErrorType.Conflict)));

        await CreateHandler().Handle(EventFor(order), CancellationToken.None);

        order.Status.Should().Be(OrderStatus.Paid);
        order.NeedsAttention.Should().BeTrue();
    }
}