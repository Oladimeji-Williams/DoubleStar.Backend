// Application/Templates/OrderTemplates.cs — NEW (kept separate so EmailTemplates.cs is untouched)
using System.Net;
using DoubleStar.SharedKernel.Contracts.Orders;

namespace DoubleStar.Modules.Notifications.Application.Templates;

internal static class OrderTemplates
{
    public static string Describe(OrderStatus status) => status switch
    {
        OrderStatus.Paid => "confirmed — we've received your payment",
        OrderStatus.Processing => "being prepared",
        OrderStatus.ReadyForPickup => "ready for pickup at our store",
        OrderStatus.OutForDelivery => "out for delivery",
        OrderStatus.Completed => "complete — thank you for shopping with us",
        OrderStatus.Cancelled => "cancelled",
        OrderStatus.Expired => "expired because payment wasn't completed",
        _ => "awaiting payment",
    };

    public static (string Subject, string Html) Email(string customerName, string orderNumber, OrderStatus status, long totalKobo)
    {
        var name = WebUtility.HtmlEncode(customerName);
        var number = WebUtility.HtmlEncode(orderNumber);

        return (
            $"Order {orderNumber}: {status}",
            $"""
            <p>Hi {name},</p>
            <p>Your order <strong>{number}</strong> ({totalKobo / 100m:N2} NGN) is now <strong>{Describe(status)}</strong>.</p>
            <p>Thanks for choosing Double Star.</p>
            """);
    }

    public static string Sms(string orderNumber, OrderStatus status) =>
        $"Double Star: order {orderNumber} is {Describe(status)}.";
}