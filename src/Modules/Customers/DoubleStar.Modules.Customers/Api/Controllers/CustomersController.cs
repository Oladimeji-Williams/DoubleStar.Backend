using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using DoubleStar.BuildingBlocks.Application.Common.Queries.Filtering;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.Modules.Customers.Api.Contracts;
using DoubleStar.Modules.Customers.Application.Commands.CreateWalkInCustomerCommand;
using DoubleStar.Modules.Customers.Application.Commands.UpdateCustomerCommand;
using DoubleStar.Modules.Customers.Application.Queries.GetCustomerByIdQuery;
using DoubleStar.Modules.Customers.Application.Queries.SearchCustomersQuery;

namespace DoubleStar.Modules.Customers.Api.Controllers;

[Authorize(Roles = "Admin,Manager,Cashier")]
public sealed class CustomersController(ISender sender)
    : V1ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateWalkIn(
        CreateWalkInCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateWalkInCustomerCommand(
                request.Name,
                request.Phone,
                request.Email),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetCustomerByIdQuery(id),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
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
                    JsonSerializer.Deserialize<
                        List<FilterCriterion>>(
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
            new SearchCustomersQuery(query),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateCustomerCommand(
                id,
                request.Name,
                request.Phone,
                request.Email,
                request.Address),
            cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : NoContent();
    }
}