// Application/EventHandlers/OrderStatusChangedEventHandler.cs — NEW
using Microsoft.Extensions.Logging;
using DoubleStar.SharedKernel.Contracts.Customers;
using DoubleStar.SharedKernel.Contracts.Orders;
using DoubleStar.Modules.Notifications.Application.Services;
using DoubleStar.Modules.Notifications.Application.Templates;

namespace DoubleStar.Modules.Notifications.Application.EventHandlers;

public sealed class OrderStatusChangedEventHandler(
    ICustomerDirectory customerDirectory,
    NotificationDispatcher dispatcher,
    ILogger<OrderStatusChangedEventHandler> logger)
    : INotificationHandler<OrderStatusChangedEvent>
{
    // Pending/Processing/Expired are either internal or noise; the customer cares about these.
    private static readonly HashSet<OrderStatus> Notifiable =
    [
        OrderStatus.Paid,
        OrderStatus.ReadyForPickup,
        OrderStatus.OutForDelivery,
        OrderStatus.Completed,
        OrderStatus.Cancelled,
    ];

    public async Task Handle(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        if (!Notifiable.Contains(notification.ToStatus))
        {
            return;
        }

        try
        {
            var customer = await customerDirectory.GetByIdAsync(notification.CustomerId, cancellationToken);
            if (customer is null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(customer.Email))
            {
                var (subject, html) = OrderTemplates.Email(
                    customer.Name ?? "Customer", notification.OrderNumber, notification.ToStatus, notification.TotalKobo);

                await dispatcher.SendEmailAsync(customer.Email, subject, html, "OrderStatusUpdate", cancellationToken);
            }

            if (!string.IsNullOrEmpty(customer.Phone))
            {
                await dispatcher.SendSmsAsync(
                    customer.Phone,
                    OrderTemplates.Sms(notification.OrderNumber, notification.ToStatus),
                    "OrderStatusUpdate",
                    cancellationToken);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // A notification problem must never roll back or fail an order transition.
            logger.LogError(ex, "Could not notify customer about order {OrderNumber}.", notification.OrderNumber);
        }
    }
}