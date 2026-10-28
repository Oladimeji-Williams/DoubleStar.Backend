using DoubleStar.SharedKernel.Contracts.Orders;
using DoubleStar.Modules.Orders.Domain.Entities;

namespace DoubleStar.Modules.Orders.Tests;

public sealed class OrderTests
{
    private static readonly DateTime Now = new(2026, 10, 17, 12, 0, 0, DateTimeKind.Utc);

    private static OrderLineDraft Line(int productId = 1, int quantity = 2, long price = 1_000_00) =>
        new(productId, "Cable", "CBL", price, quantity, false, []);

    private static Order Place(FulfillmentMethod method = FulfillmentMethod.Pickup, long fee = 2_000_00) =>
        Order.Place(
            Guid.NewGuid(), method, "Ada", "+2348011112222",
            method == FulfillmentMethod.Delivery ? "1 Allen Ave" : null,
            method == FulfillmentMethod.Delivery ? "Ikeja" : null,
            method == FulfillmentMethod.Delivery ? "Lagos" : null,
            notes: null, idempotencyKey: null, deliveryFeeKobo: fee,
            expiresAtUtc: Now.AddMinutes(30), lines: [Line()]);

    [Fact]
    public void Place_ComputesTotals_AndStartsPendingPayment()
    {
        var order = Place(FulfillmentMethod.Delivery);

        order.Status.Should().Be(OrderStatus.PendingPayment);
        order.SubtotalKobo.Should().Be(2_000_00);
        order.DeliveryFeeKobo.Should().Be(2_000_00);
        order.TotalKobo.Should().Be(4_000_00);
    }

    [Fact]
    public void Place_Pickup_IgnoresDeliveryFee()
    {
        var order = Place(FulfillmentMethod.Pickup, fee: 5_000_00);

        order.DeliveryFeeKobo.Should().Be(0);
        order.TotalKobo.Should().Be(order.SubtotalKobo);
    }

    [Fact]
    public void Place_DeliveryWithoutAddress_Throws()
    {
        var act = () => Order.Place(
            Guid.NewGuid(), FulfillmentMethod.Delivery, "Ada", "+2348011112222",
            null, null, null, null, null, 0, Now.AddMinutes(30), [Line()]);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Place_WithNoLines_Throws()
    {
        var act = () => Order.Place(
            Guid.NewGuid(), FulfillmentMethod.Pickup, "Ada", "+2348011112222",
            null, null, null, null, null, 0, Now.AddMinutes(30), []);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkPaid_MovesToPaid_AndRecordsHistory()
    {
        var order = Place();

        order.MarkPaid(Guid.NewGuid(), "REF-1", Now);

        order.Status.Should().Be(OrderStatus.Paid);
        order.PaidAtUtc.Should().Be(Now);
        order.StatusHistory.Should().ContainSingle(h => h.ToStatus == OrderStatus.Paid);
    }

    [Fact]
    public void MarkPaid_Twice_Throws()
    {
        var order = Place();
        order.MarkPaid(Guid.NewGuid(), "REF-1", Now);

        var act = () => order.MarkPaid(Guid.NewGuid(), "REF-1", Now);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Pickup_HappyPath_ReachesCompleted()
    {
        var order = Place(FulfillmentMethod.Pickup);
        order.MarkPaid(Guid.NewGuid(), "REF", Now);

        order.Advance(OrderStatus.Processing);
        order.Advance(OrderStatus.ReadyForPickup);
        order.Advance(OrderStatus.Completed);

        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public void Delivery_CannotBeMarkedReadyForPickup()
    {
        var order = Place(FulfillmentMethod.Delivery);
        order.MarkPaid(Guid.NewGuid(), "REF", Now);
        order.Advance(OrderStatus.Processing);

        var act = () => order.Advance(OrderStatus.ReadyForPickup);

        act.Should().Throw<InvalidOperationException>();
        order.StaffNextStatuses.Should().Equal(OrderStatus.OutForDelivery);
    }

    [Fact]
    public void Advance_CannotSkipPaymentOrCancel()
    {
        var order = Place();

        var skip = () => order.Advance(OrderStatus.Processing);
        var cancel = () => order.Advance(OrderStatus.Cancelled);

        skip.Should().Throw<InvalidOperationException>();
        cancel.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_OnlyWorksBeforePayment()
    {
        var pending = Place();
        pending.Cancel("changed my mind");
        pending.Status.Should().Be(OrderStatus.Cancelled);

        var paid = Place();
        paid.MarkPaid(Guid.NewGuid(), "REF", Now);
        var act = () => paid.Cancel("too late");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void IsExpired_OnlyForPendingOrdersPastTheirWindow()
    {
        var order = Place();

        order.IsExpired(Now.AddMinutes(29)).Should().BeFalse();
        order.IsExpired(Now.AddMinutes(30)).Should().BeTrue();

        order.MarkPaid(Guid.NewGuid(), "REF", Now);
        order.IsExpired(Now.AddHours(5)).Should().BeFalse();
    }
}