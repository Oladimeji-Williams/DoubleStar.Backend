using System.Text.Json;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using DoubleStar.BuildingBlocks.Application.Common.Queries.Filtering;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.BuildingBlocks.Infrastructure.Api;

using DoubleStar.Modules.Reviews.Api.Contracts;
using DoubleStar.Modules.Reviews.Application.Commands.ApproveReviewCommand;
using DoubleStar.Modules.Reviews.Application.Commands.DeleteReviewCommand;
using DoubleStar.Modules.Reviews.Application.Commands.RejectReviewCommand;
using DoubleStar.Modules.Reviews.Application.Commands.SubmitReviewCommand;
using DoubleStar.Modules.Reviews.Application.Queries.GetApprovedReviewsQuery;
using DoubleStar.Modules.Reviews.Application.Queries.GetPendingReviewsQuery;
using DoubleStar.Modules.Reviews.Application.Queries.GetAllReviewsQuery;

namespace DoubleStar.Modules.Reviews.Api.Controllers;

public sealed class ReviewsController(ISender sender)
    : V1ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetApproved(
        CancellationToken cancellationToken)
    {
        var result =
            await sender.Send(
                new GetApprovedReviewsQuery(),
                cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetPending(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] string? filters = null,
        CancellationToken cancellationToken = default)
    {
        var parsedFilters =
            ParseFilters(filters);

        if (parsedFilters.IsInvalid)
        {
            return BadRequest("Invalid filters.");
        }

        var query = new PagedQuery(
            Page: page,
            PageSize: pageSize,
            Search: search,
            SortBy: sortBy,
            SortDescending: sortDescending,
            Filters: parsedFilters.Filters);

        var result =
            await sender.Send(
                new GetPendingReviewsQuery(query),
                cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Submit(
        SubmitReviewRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await sender.Send(
                new SubmitReviewCommand(
                    request.Rating,
                    request.Comment),
                cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Approve(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result =
            await sender.Send(
                new ApproveReviewCommand(id),
                cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Reject(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result =
            await sender.Send(
                new RejectReviewCommand(id),
                cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result =
            await sender.Send(
                new DeleteReviewCommand(id),
                cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : NoContent();
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] string? filters = null,
        CancellationToken cancellationToken = default)
    {
        var parsedFilters =
            ParseFilters(filters);

        if (parsedFilters.IsInvalid)
        {
            return BadRequest("Invalid filters.");
        }

        var query = new PagedQuery(
            Page: page,
            PageSize: pageSize,
            Search: search,
            SortBy: sortBy,
            SortDescending: sortDescending,
            Filters: parsedFilters.Filters);

        var result =
            await sender.Send(
                new GetAllReviewsQuery(query),
                cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }

    private static (
        IReadOnlyList<FilterCriterion>? Filters,
        bool IsInvalid)
        ParseFilters(string? filters)
    {
        if (string.IsNullOrWhiteSpace(filters))
        {
            return (null, false);
        }

        try
        {
            var parsed =
                JsonSerializer.Deserialize<List<FilterCriterion>>(
                    filters);

            return (parsed, false);
        }
        catch (JsonException)
        {
            return (null, true);
        }
    }
}