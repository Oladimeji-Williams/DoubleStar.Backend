using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.Queries.GetOrderById;
using DoubleStar.Modules.Orders.Domain.Entities;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Orders;

namespace DoubleStar.Modules.Orders.Tests;

public sealed class GetOrderByIdQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orders = new();
    private readonly Guid _ownerId = Guid.NewGuid();

    private GetOrderByIdQueryHandler CreateHandler(Mock<ICurrentUser> user) =>
        new(_orders.Object, user.Object, new FixedTimeProvider(OrderTestSupport.Now));

    private void GivenStoredOrder(Order order) =>
        _orders.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);

    [Fact]
    public async Task Handle_ForTheOwner_ReturnsTheOrder()
    {
        GivenStoredOrder(OrderTestSupport.PendingOrder(_ownerId));

        var result = await CreateHandler(OrderTestSupport.Customer(_ownerId))
            .Handle(new GetOrderByIdQuery(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(OrderStatus.PendingPayment);
    }

    [Fact]
    public async Task Handle_ForADifferentCustomer_ReportsNotFoundInsteadOfLeakingTheOrder()
    {
        GivenStoredOrder(OrderTestSupport.PendingOrder(_ownerId));

        var result = await CreateHandler(OrderTestSupport.Customer(Guid.NewGuid()))
            .Handle(new GetOrderByIdQuery(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "Orders.NotFound");
    }

    [Fact]
    public async Task Handle_ForStaff_ReturnsAnyCustomersOrder()
    {
        GivenStoredOrder(OrderTestSupport.PendingOrder(_ownerId));

        var result = await CreateHandler(OrderTestSupport.Staff(Guid.NewGuid()))
            .Handle(new GetOrderByIdQuery(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenNobodyIsSignedIn_FailsWithoutTouchingTheRepository()
    {
        var result = await CreateHandler(OrderTestSupport.Anonymous())
            .Handle(new GetOrderByIdQuery(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        _orders.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}