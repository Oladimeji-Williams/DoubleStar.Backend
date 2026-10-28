using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using DoubleStar.BuildingBlocks.Infrastructure.Api;

using DoubleStar.Modules.Notifications.Api.Contracts;
using DoubleStar.Modules.Notifications.Application.Commands.SendLowStockAlertCommand;
using DoubleStar.Modules.Notifications.Application.Commands.SendRepairStatusUpdateCommand;
using DoubleStar.Modules.Notifications.Application.Commands.SendSaleReceiptCommand;
using DoubleStar.Modules.Notifications.Application.Queries.GetNotificationLogQuery;
using System.Text.Json;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Filtering;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;

namespace DoubleStar.Modules.Notifications.Api.Controllers;

[Authorize(Roles = "Admin,Manager")]
public sealed class NotificationsController(ISender sender)
    : V1ControllerBase
{
    [HttpPost("repairs/{ticketId:int}/resend")]
    public async Task<IActionResult> ResendRepairUpdate(
        int ticketId,
        CancellationToken cancellationToken)
    {
        var result =
            await sender.Send(
                new SendRepairStatusUpdateCommand(ticketId),
                cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : NoContent();
    }

    [HttpPost("sales/receipt")]
    public async Task<IActionResult> SendReceipt(
        SendSaleReceiptRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await sender.Send(
                new SendSaleReceiptCommand(
                    request.CustomerId,
                    request.SaleId,
                    request.TotalKobo),
                cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : NoContent();
    }

    [HttpPost("low-stock-alert")]
    public async Task<IActionResult> SendLowStockAlert(
        SendLowStockAlertRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await sender.Send(
                new SendLowStockAlertCommand(
                    request.ProductName,
                    request.AvailableQuantity,
                    request.StaffEmail),
                cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : NoContent();
    }

    [HttpGet("log")]
    public async Task<IActionResult> GetLog(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] string? filters = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<FilterCriterion>? parsedFilters = null;

        if (!string.IsNullOrWhiteSpace(filters))
        {
            try
            {
                parsedFilters =
                    JsonSerializer.Deserialize<List<FilterCriterion>>(
                        filters);
            }
            catch (JsonException)
            {
                return BadRequest("Invalid filters.");
            }
        }

        var query = new PagedQuery(
            Page: page,
            PageSize: pageSize,
            Search: search,
            SortBy: sortBy,
            SortDescending: sortDescending,
            Filters: parsedFilters);

        var result =
            await sender.Send(
                new GetNotificationLogQuery(query),
                cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }
}