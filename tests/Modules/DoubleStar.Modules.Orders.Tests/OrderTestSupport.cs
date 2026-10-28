using DoubleStar.Modules.Orders.Domain.Entities;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Orders;

namespace DoubleStar.Modules.Orders.Tests;

internal static class OrderTestSupport
{
    public static readonly DateTime Now = new(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>An unpaid pickup order: 2 x N1,500 of product 7.</summary>
    public static Order PendingOrder(Guid customerId, DateTime? expiresAtUtc = null) =>
        Order.Place(
            customerId,
            FulfillmentMethod.Pickup,
            "Ada",
            "+2348011112222",
            null, null, null, null, null,
            0,
            expiresAtUtc ?? Now.AddMinutes(30),
            [new OrderLineDraft(7, "Cable", "CBL", 150_000, 2, false, [])]);

    public static Order PaidOrder(Guid customerId)
    {
        var order = PendingOrder(customerId);
        order.MarkPaid(Guid.NewGuid(), "REF-1", Now);
        return order;
    }

    public static Mock<ICurrentUser> Customer(Guid id) => User(id, isStaff: false);

    public static Mock<ICurrentUser> Staff(Guid id) => User(id, isStaff: true);

    public static Mock<ICurrentUser> Anonymous()
    {
        var user = new Mock<ICurrentUser>();
        user.SetupGet(u => u.UserId).Returns((Guid?)null);
        return user;
    }

    private static Mock<ICurrentUser> User(Guid id, bool isStaff)
    {
        var user = new Mock<ICurrentUser>();
        user.SetupGet(u => u.UserId).Returns(id);
        user.SetupGet(u => u.IsStaff).Returns(isStaff);
        return user;
    }
}

internal sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
}