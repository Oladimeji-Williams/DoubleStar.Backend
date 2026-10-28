using DoubleStar.SharedKernel.Contracts.Orders;
using DoubleStar.SharedKernel.Domain;

namespace DoubleStar.Modules.Orders.Domain.Entities;

public sealed class Order : Entity
{
    // Cancelling a PAID order is deliberately absent: it implies a refund + restock, which is a
    // staff workflow (Payments refund, Inventory restock) rather than a state flip.
    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        [OrderStatus.PendingPayment] = [OrderStatus.Paid, OrderStatus.Cancelled, OrderStatus.Expired],
        [OrderStatus.Paid] = [OrderStatus.Processing],
        [OrderStatus.Processing] = [OrderStatus.ReadyForPickup, OrderStatus.OutForDelivery],
        [OrderStatus.ReadyForPickup] = [OrderStatus.Completed],
        [OrderStatus.OutForDelivery] = [OrderStatus.Completed],
        [OrderStatus.Completed] = [],
        [OrderStatus.Cancelled] = [],
        [OrderStatus.Expired] = [],
    };

    private readonly List<OrderLine> _lines = [];
    private readonly List<OrderStatusHistory> _statusHistory = [];

    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public FulfillmentMethod FulfillmentMethod { get; private set; }

    public long SubtotalKobo { get; private set; }
    public long DeliveryFeeKobo { get; private set; }
    public long TotalKobo { get; private set; }

    public string ContactName { get; private set; } = null!;
    public string ContactPhone { get; private set; } = null!;
    public string? DeliveryAddress { get; private set; }
    public string? DeliveryCity { get; private set; }
    public string? DeliveryState { get; private set; }
    public string? Notes { get; private set; }

    public string? IdempotencyKey { get; private set; }
    public string? PaymentReference { get; private set; }
    public Guid? PaymentId { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }

    public bool NeedsAttention { get; private set; }
    public string? AttentionReason { get; private set; }

    public IReadOnlyList<OrderLine> Lines => _lines;
    public IReadOnlyList<OrderStatusHistory> StatusHistory => _statusHistory;

    public string OrderNumber => $"DS-{Id:D6}";
    public bool CanBeCancelled => Status == OrderStatus.PendingPayment;

    /// <summary>Transitions a staff member may trigger by hand (system-driven ones are excluded).</summary>
    public IReadOnlyList<OrderStatus> StaffNextStatuses =>
        AllowedTransitions[Status]
            .Where(IsAllowed)
            .Where(next => next is not (OrderStatus.Paid or OrderStatus.Cancelled or OrderStatus.Expired))
            .ToList();

    private Order() { }

    public static Order Place(
        Guid customerId,
        FulfillmentMethod fulfillmentMethod,
        string contactName,
        string contactPhone,
        string? deliveryAddress,
        string? deliveryCity,
        string? deliveryState,
        string? notes,
        string? idempotencyKey,
        long deliveryFeeKobo,
        DateTime expiresAtUtc,
        IReadOnlyCollection<OrderLineDraft> lines)
    {
        if (lines.Count == 0)
        {
            throw new InvalidOperationException("An order needs at least one line.");
        }

        if (fulfillmentMethod == FulfillmentMethod.Delivery &&
            (string.IsNullOrWhiteSpace(deliveryAddress) || string.IsNullOrWhiteSpace(deliveryCity) || string.IsNullOrWhiteSpace(deliveryState)))
        {
            throw new InvalidOperationException("A delivery order needs an address, city and state.");
        }

        if (deliveryFeeKobo < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deliveryFeeKobo));
        }

        var order = new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.PendingPayment,
            FulfillmentMethod = fulfillmentMethod,
            ContactName = contactName.Trim(),
            ContactPhone = contactPhone.Trim(),
            DeliveryAddress = fulfillmentMethod == FulfillmentMethod.Delivery ? deliveryAddress?.Trim() : null,
            DeliveryCity = fulfillmentMethod == FulfillmentMethod.Delivery ? deliveryCity?.Trim() : null,
            DeliveryState = fulfillmentMethod == FulfillmentMethod.Delivery ? deliveryState?.Trim() : null,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim(),
            DeliveryFeeKobo = fulfillmentMethod == FulfillmentMethod.Delivery ? deliveryFeeKobo : 0,
            ExpiresAtUtc = expiresAtUtc,
        };

        foreach (var draft in lines)
        {
            order._lines.Add(OrderLine.Create(draft));
        }

        order.SubtotalKobo = order._lines.Sum(l => l.LineTotalKobo);
        order.TotalKobo = order.SubtotalKobo + order.DeliveryFeeKobo;

        return order;
    }

    public bool IsExpired(DateTime utcNow) => Status == OrderStatus.PendingPayment && utcNow >= ExpiresAtUtc;

    public void AttachPayment(string reference)
    {
        EnsureStatus(OrderStatus.PendingPayment, "Payment can only be started for an order awaiting payment.");
        PaymentReference = reference;
    }

    public void ExtendPaymentWindow(DateTime newExpiryUtc)
    {
        EnsureStatus(OrderStatus.PendingPayment, "Only an order awaiting payment can be extended.");

        if (newExpiryUtc > ExpiresAtUtc)
        {
            ExpiresAtUtc = newExpiryUtc;
        }
    }

    public void MarkPaid(Guid paymentId, string reference, DateTime paidAtUtc)
    {
        EnsureStatus(OrderStatus.PendingPayment, "Only an order awaiting payment can be marked paid.");

        PaymentId = paymentId;
        PaymentReference = reference;
        PaidAtUtc = paidAtUtc;
        TransitionTo(OrderStatus.Paid, "Payment received");
    }

    public void MarkLineStockConsumed(int lineId)
    {
        var line = _lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new InvalidOperationException($"Line '{lineId}' does not belong to this order.");

        line.MarkStockConsumed();
    }

    /// <summary>Staff-driven progress: Paid → Processing → Ready/Out for delivery → Completed.</summary>
    public void Advance(OrderStatus next)
    {
        if (!StaffNextStatuses.Contains(next))
        {
            throw new InvalidOperationException($"An order that is {Status} cannot move to {next}.");
        }

        TransitionTo(next, note: null);
    }

    public void Cancel(string reason)
    {
        EnsureStatus(OrderStatus.PendingPayment, "Only an order awaiting payment can be cancelled here. For a paid order, refund it from Payments.");
        TransitionTo(OrderStatus.Cancelled, reason);
    }

    public void Expire()
    {
        EnsureStatus(OrderStatus.PendingPayment, "Only an order awaiting payment can expire.");
        TransitionTo(OrderStatus.Expired, "Payment window elapsed");
    }

    /// <summary>Flags a situation a human must resolve (e.g. payment arrived after cancellation). Idempotent.</summary>
    public void FlagForAttention(string reason)
    {
        NeedsAttention = true;
        AttentionReason = reason.Length > 500 ? reason[..500] : reason;
    }

    private bool IsAllowed(OrderStatus next) =>
        AllowedTransitions[Status].Contains(next) &&
        !(next == OrderStatus.ReadyForPickup && FulfillmentMethod != FulfillmentMethod.Pickup) &&
        !(next == OrderStatus.OutForDelivery && FulfillmentMethod != FulfillmentMethod.Delivery);

    private void EnsureStatus(OrderStatus expected, string message)
    {
        if (Status != expected)
        {
            throw new InvalidOperationException($"{message} (current status: {Status}).");
        }
    }

    private void TransitionTo(OrderStatus next, string? note)
    {
        if (!IsAllowed(next))
        {
            throw new InvalidOperationException($"Cannot move an order from {Status} to {next}.");
        }

        _statusHistory.Add(OrderStatusHistory.Create(Status, next, note));
        Status = next;
    }
}