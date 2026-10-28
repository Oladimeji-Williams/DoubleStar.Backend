
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Filtering;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.BuildingBlocks.Infrastructure.Api;

using DoubleStar.Modules.Sales.Api.Contracts;
using DoubleStar.Modules.Sales.Application.Commands.AddSaleLineCommand;
using DoubleStar.Modules.Sales.Application.Commands.CompleteSaleCommand;
using DoubleStar.Modules.Sales.Application.Commands.CreateSaleCommand;
using DoubleStar.Modules.Sales.Application.Commands.VoidSaleCommand;
using DoubleStar.Modules.Sales.Application.Queries.GetAllSalesQuery;
using DoubleStar.Modules.Sales.Application.Queries.GetSaleByIdQuery;
using DoubleStar.Modules.Sales.Application.Queries.GetSalesByCustomerQuery;

namespace DoubleStar.Modules.Sales.Api.Controllers;

public sealed class SalesController(
    ISender sender,
    ICurrentUser currentUser)
    : V1ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> Create(
        CreateSaleRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await sender.Send(
                new CreateSaleCommand(
                    request.CustomerId,
                    request.WalkInName,
                    request.WalkInPhone),
                cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpPost("{id:int}/lines")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> AddLine(
        int id,
        AddSaleLineRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await sender.Send(
                new AddSaleLineCommand(
                    id,
                    request.ProductId,
                    request.Quantity,
                    request.SerialNumber),
                cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpPost("{id:int}/complete")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> Complete(
        int id,
        CancellationToken cancellationToken)
    {
        var result =
            await sender.Send(
                new CompleteSaleCommand(id),
                cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpPost("{id:int}/void")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> Void(
        int id,
        CancellationToken cancellationToken)
    {
        var result =
            await sender.Send(
                new VoidSaleCommand(id),
                cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : NoContent();
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result =
            await sender.Send(
                new GetSaleByIdQuery(id),
                cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> GetAll(
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
                    System.Text.Json.JsonSerializer
                        .Deserialize<List<FilterCriterion>>(filters);
            }
            catch (System.Text.Json.JsonException)
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
                new GetAllSalesQuery(query),
                cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpGet("customers/{customerId:guid}")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> GetByCustomer(
        Guid customerId,
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
                    System.Text.Json.JsonSerializer
                        .Deserialize<List<FilterCriterion>>(filters);
            }
            catch (System.Text.Json.JsonException)
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
                new GetSalesByCustomerQuery(
                    customerId,
                    query),
                cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMine(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] string? filters = null,
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return Unauthorized();
        }

        IReadOnlyList<FilterCriterion>? parsedFilters = null;

        if (!string.IsNullOrWhiteSpace(filters))
        {
            try
            {
                parsedFilters =
                    System.Text.Json.JsonSerializer
                        .Deserialize<List<FilterCriterion>>(filters);
            }
            catch (System.Text.Json.JsonException)
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
                new GetSalesByCustomerQuery(
                    currentUser.UserId.Value,
                    query),
                cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }
}