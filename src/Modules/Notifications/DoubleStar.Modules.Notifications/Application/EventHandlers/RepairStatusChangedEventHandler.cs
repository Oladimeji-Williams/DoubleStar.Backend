// Application/EventHandlers/RepairStatusChangedEventHandler.cs
using DoubleStar.SharedKernel.Contracts.Customers;
using DoubleStar.SharedKernel.Contracts.Repairs;
using DoubleStar.Modules.Notifications.Application.Services;
using DoubleStar.Modules.Notifications.Application.Templates;

namespace DoubleStar.Modules.Notifications.Application.EventHandlers;

public sealed class RepairStatusChangedEventHandler(ICustomerDirectory customerDirectory, NotificationDispatcher dispatcher)
    : INotificationHandler<RepairStatusChangedEvent>
{
    public async Task Handle(RepairStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.CustomerId is null)
        {
            return;
        }

        var customer = await customerDirectory.GetByIdAsync(notification.CustomerId.Value, cancellationToken);
        if (customer is null)
        {
            return;
        }

        var (subject, html) = EmailTemplates.RepairStatusUpdate(customer.Name, notification.TicketId, notification.ToStatus);

        if (!string.IsNullOrEmpty(customer.Email))
        {
            await dispatcher.SendEmailAsync(customer.Email, subject, html, "RepairStatusUpdate", cancellationToken);
        }

        if (!string.IsNullOrEmpty(customer.Phone))
        {
            var smsMessage = SmsTemplates.RepairStatusUpdate(notification.TicketId, notification.ToStatus);
            await dispatcher.SendSmsAsync(customer.Phone, smsMessage, "RepairStatusUpdate", cancellationToken);
        }
    }
}