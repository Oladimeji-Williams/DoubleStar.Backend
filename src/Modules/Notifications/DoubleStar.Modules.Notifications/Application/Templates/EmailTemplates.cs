// Application/Templates/EmailTemplates.cs
using DoubleStar.SharedKernel.Contracts.Repairs;

namespace DoubleStar.Modules.Notifications.Application.Templates;

internal static class EmailTemplates
{
    public static (string Subject, string Html) RepairStatusUpdate(string customerName, int ticketId, RepairStatus newStatus) =>
    (
        $"Update on your repair #{ticketId}",
        $"""
        <p>Hi {customerName},</p>
        <p>Your repair ticket <strong>#{ticketId}</strong> is now <strong>{newStatus}</strong>.</p>
        <p>Thanks for choosing Double Star.</p>
        """
    );

    public static (string Subject, string Html) SaleReceipt(string customerName, int saleId, long totalKobo) =>
    (
        $"Your receipt from Double Star — Sale #{saleId}",
        $"""
        <p>Hi {customerName},</p>
        <p>Thanks for your purchase — total <strong>{totalKobo / 100m:N2} NGN</strong> (Sale #{saleId}).</p>
        """
    );

    public static (string Subject, string Html) LowStockAlert(string productName, int availableQuantity) =>
    (
        $"Low stock: {productName}",
        $"<p><strong>{productName}</strong> has only <strong>{availableQuantity}</strong> unit(s) left.</p>"
    );
}