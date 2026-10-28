//
// The webhook's consumer. Flow:  Paystack -> POST /PaystackWebhook/webhook -> Payments verifies + marks
// the transaction Successful -> publishes PaymentSucceededEvent -> THIS turns reserved stock into sold
// stock and moves the order to Paid -> publishes OrderStatusChangedEvent -> Notifications emails the customer.
//
// MUST be idempotent: the webhook is retried, and the callback verify races it.
using Microsoft.Extensions.Logging;
using DoubleStar.SharedKernel.Contracts.Orders;
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.Mappings;
using DoubleStar.Modules.Orders.Application.Services;

namespace DoubleStar.Modules.Orders.Application.EventHandlers;

public sealed class PaymentSucceededEventHandler(
    IOrderRepository orderRepository,
    OrderStockService orderStockService,
    IPublisher publisher,
    TimeProvider timeProvider,
    ILogger<PaymentSucceededEventHandler> logger)
    : INotificationHandler<PaymentSucceededEvent>
{
    public async Task Handle(PaymentSucceededEvent notification, CancellationToken cancellationToken)
    {
        if (notification.SourceType != PaymentSourceType.Order)
        {
            return;
        }

        var order = await orderRepository.GetByIdAsync(notification.SourceId, cancellationToken);
        if (order is null)
        {
            logger.LogError(
                "Payment {Reference} succeeded for order {OrderId}, which does not exist.",
                notification.Reference, notification.SourceId);
            return;
        }

        switch (order.Status)
        {
            case OrderStatus.PendingPayment:
                break;

            case OrderStatus.Cancelled or OrderStatus.Expired:
                // Money arrived for an order we'd already given up on. Stock was released and may be gone.
                order.FlagForAttention(
                    $"Payment {notification.Reference} was received after the order was {order.Status}. Refund it or re-create the order.");
                await orderRepository.SaveAsync(order, cancellationToken);
                logger.LogWarning("Late payment {Reference} on {Status} order {OrderId}.",
                    notification.Reference, order.Status, order.Id);
                return;

            default:
                return; // Paid / Processing / ... — this is a redelivery, nothing to do.
        }

        if (notification.AmountKobo != order.TotalKobo)
        {
            order.FlagForAttention(
                $"Paid {notification.AmountKobo / 100m:N2} but the order total is {order.TotalKobo / 100m:N2}.");
        }

        var consumed = await orderStockService.ConsumeAsync(order, cancellationToken);
        if (consumed.IsFailure)
        {
            // The customer HAS paid, so we still mark the order Paid — but a human must look at the stock.
            order.FlagForAttention(
                $"Payment received but stock could not be finalised: {string.Join("; ", consumed.Errors.Select(e => e.Code))}.");
            logger.LogError("Stock consumption failed for paid order {OrderId}.", order.Id);
        }

        var from = order.Status;
        order.MarkPaid(notification.PaymentId, notification.Reference, timeProvider.GetUtcNow().UtcDateTime);
        await orderRepository.SaveAsync(order, cancellationToken);

        await publisher.Publish(order.ToStatusChangedEvent(from), cancellationToken);
    }
}