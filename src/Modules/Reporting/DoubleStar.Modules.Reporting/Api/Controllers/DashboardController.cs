// Api/Controllers/DashboardController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.Modules.Reporting.Application.Queries.GetInventoryValuationQuery;
using DoubleStar.Modules.Reporting.Application.Queries.GetRepairTurnaroundQuery;
using DoubleStar.Modules.Reporting.Application.Queries.GetRevenueByPeriodQuery;
using DoubleStar.Modules.Reporting.Application.Queries.GetSalesSummaryQuery;

namespace DoubleStar.Modules.Reporting.Api.Controllers;

[Authorize(Roles = "Admin,Manager")]
public sealed class DashboardController(ISender sender) : V1ControllerBase
{
    [HttpGet("sales-summary")]
    public async Task<IActionResult> GetSalesSummary(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSalesSummaryQuery(from, to), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpGet("revenue-by-period")]
    public async Task<IActionResult> GetRevenueByPeriod(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRevenueByPeriodQuery(from, to), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpGet("inventory-valuation")]
    public async Task<IActionResult> GetInventoryValuation(
        [FromQuery] int lowStockThreshold, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetInventoryValuationQuery(lowStockThreshold == 0 ? 5 : lowStockThreshold), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpGet("repair-turnaround")]
    public async Task<IActionResult> GetRepairTurnaround(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRepairTurnaroundQuery(from, to), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }
}