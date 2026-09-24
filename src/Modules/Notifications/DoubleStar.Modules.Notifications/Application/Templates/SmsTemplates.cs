// Application/Templates/SmsTemplates.cs
using DoubleStar.SharedKernel.Contracts.Repairs;

namespace DoubleStar.Modules.Notifications.Application.Templates;

internal static class SmsTemplates
{
    public static string RepairStatusUpdate(int ticketId, RepairStatus newStatus) =>
        $"Double Star: your repair #{ticketId} is now {newStatus}.";
}