using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.Modules.Inventory.Api.Contracts;
using DoubleStar.Modules.Inventory.Application.Commands.AddSerializedUnitCommand;
using DoubleStar.Modules.Inventory.Application.Commands.AdjustStockCommand;
using DoubleStar.Modules.Inventory.Application.Commands.RestockBulkCommand;
using DoubleStar.Modules.Inventory.Application.Queries.GetLowStockQuery;
using DoubleStar.Modules.Inventory.Application.Queries.GetSerializedUnitQuery;
using DoubleStar.Modules.Inventory.Application.Queries.GetStockLevelQuery;
using DoubleStar.Modules.Inventory.Application.Queries.GetStockLevelsQuery;
using DoubleStar.Modules.Inventory.Application.Queries.GetPagedInventoryOverview;
using System.Text.Json;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Filtering;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;

namespace DoubleStar.Modules.Inventory.Api.Controllers;

[Authorize(Roles = "Admin,Manager")]
public sealed class StockController(ISender sender) : V1ControllerBase
{
    [HttpGet("{productId:int}")]
    public async Task<IActionResult> GetLevel(int productId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStockLevelQuery(productId), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpGet("low")]
    public async Task<IActionResult> GetLow([FromQuery] int threshold, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLowStockQuery(threshold), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpGet("serials/{serialNumber}")]
    public async Task<IActionResult> GetSerial(string serialNumber, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSerializedUnitQuery(serialNumber), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpPost("restock")]
    public async Task<IActionResult> RestockBulk(RestockBulkRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RestockBulkCommand(request.ProductId, request.Quantity, request.Reference), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    [HttpPost("serials")]
    public async Task<IActionResult> AddSerial(AddSerializedUnitRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new AddSerializedUnitCommand(request.ProductId, request.SerialNumber), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust(AdjustStockRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new AdjustStockCommand(request.ProductId, request.NewQuantity, request.Reason), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    [HttpGet("by-products")]
    public async Task<IActionResult> GetByProducts(
        [FromQuery] int[] productIds,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetStockLevelsQuery(productIds),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(
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

        var result = await sender.Send(
            new GetPagedInventoryOverviewQuery(query),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }
}