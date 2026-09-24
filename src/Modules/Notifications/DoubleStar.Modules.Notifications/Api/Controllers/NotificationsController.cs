// Api/Controllers/NotificationsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.Modules.Notifications.Api.Contracts;
using DoubleStar.Modules.Notifications.Application.Commands.SendLowStockAlertCommand;
using DoubleStar.Modules.Notifications.Application.Commands.SendRepairStatusUpdateCommand;
using DoubleStar.Modules.Notifications.Application.Commands.SendSaleReceiptCommand;
using DoubleStar.Modules.Notifications.Application.Queries.GetNotificationLogQuery;

namespace DoubleStar.Modules.Notifications.Api.Controllers;

[Authorize(Roles = "Admin,Manager")]
public sealed class NotificationsController(ISender sender) : V1ControllerBase
{
    [HttpPost("repairs/{ticketId:int}/resend")]
    public async Task<IActionResult> ResendRepairUpdate(int ticketId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SendRepairStatusUpdateCommand(ticketId), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    [HttpPost("sales/receipt")]
    public async Task<IActionResult> SendReceipt(SendSaleReceiptRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SendSaleReceiptCommand(request.CustomerId, request.SaleId, request.TotalKobo), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    [HttpPost("low-stock-alert")]
    public async Task<IActionResult> SendLowStockAlert(SendLowStockAlertRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SendLowStockAlertCommand(request.ProductName, request.AvailableQuantity, request.StaffEmail), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    [HttpGet("log")]
    public async Task<IActionResult> GetLog([FromQuery] int take, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNotificationLogQuery(take == 0 ? 100 : take), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }
}