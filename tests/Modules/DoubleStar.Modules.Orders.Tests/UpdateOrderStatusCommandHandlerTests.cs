using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.Commands.UpdateOrderStatus;
using DoubleStar.Modules.Orders.Domain.Entities;
using DoubleStar.SharedKernel.Contracts.Orders;
using MediatR;

namespace DoubleStar.Modules.Orders.Tests;

public sealed class UpdateOrderStatusCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orders = new();
    private readonly Mock<IPublisher> _publisher = new();

    private UpdateOrderStatusCommandHandler CreateHandler() => new(_orders.Object, _publisher.Object);

    private void GivenStoredOrder(Order order) =>
        _orders.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);

    [Fact]
    public async Task Handle_AdvancesAPaidOrder_SavesItAndAnnouncesTheChange()
    {
        var order = OrderTestSupport.PaidOrder(Guid.NewGuid());
        GivenStoredOrder(order);

        var result = await CreateHandler()
            .Handle(new UpdateOrderStatusCommand(1, OrderStatus.Processing), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Processing);
        _orders.Verify(r => r.SaveAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(
            p => p.Publish(
                It.Is<OrderStatusChangedEvent>(e => e.ToStatus == OrderStatus.Processing),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_RefusesAMoveThatSkipsAStep()
    {
        var order = OrderTestSupport.PaidOrder(Guid.NewGuid());
        GivenStoredOrder(order);

        var result = await CreateHandler()
            .Handle(new UpdateOrderStatusCommand(1, OrderStatus.Completed), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "Orders.InvalidTransition");
        order.Status.Should().Be(OrderStatus.Paid);
        _orders.Verify(r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisher.Verify(
            p => p.Publish(It.IsAny<OrderStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DoesNotLetStaffCancelThroughTheStatusEndpoint()
    {
        var order = OrderTestSupport.PendingOrder(Guid.NewGuid());
        GivenStoredOrder(order);

        var result = await CreateHandler()
            .Handle(new UpdateOrderStatusCommand(1, OrderStatus.Cancelled), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.PendingPayment);
    }

    [Fact]
    public async Task Handle_ForAnUnknownOrder_ReportsNotFound()
    {
        _orders
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await CreateHandler()
            .Handle(new UpdateOrderStatusCommand(99, OrderStatus.Processing), CancellationToken.None);

        result.Errors.Should().ContainSingle(e => e.Code == "Orders.NotFound");
    }
}