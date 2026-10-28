using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.Queries.GetMyOrders;
using DoubleStar.Modules.Orders.Domain.Entities;

namespace DoubleStar.Modules.Orders.Tests;

public sealed class GetMyOrdersQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orders = new();

    [Fact]
    public async Task Handle_OnlyAsksForTheSignedInCustomersOrders()
    {
        var customerId = Guid.NewGuid();
        var query = new PagedQuery(1, 25, null, null, false, null);
        _orders
            .Setup(r => r.SearchForCustomerAsync(customerId, query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Order>([OrderTestSupport.PendingOrder(customerId)], 1, 25, 1));
        var handler = new GetMyOrdersQueryHandler(_orders.Object, OrderTestSupport.Customer(customerId).Object);

        var result = await handler.Handle(new GetMyOrdersQuery(query), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().ContainSingle();
        result.Value.TotalCount.Should().Be(1);
        _orders.Verify(r => r.SearchAsync(It.IsAny<PagedQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNobodyIsSignedIn_Fails()
    {
        var handler = new GetMyOrdersQueryHandler(_orders.Object, OrderTestSupport.Anonymous().Object);

        var result = await handler.Handle(
            new GetMyOrdersQuery(new PagedQuery(1, 25, null, null, false, null)),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}