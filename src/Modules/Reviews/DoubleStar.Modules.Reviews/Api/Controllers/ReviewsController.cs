// Modules/Reviews/Api/Controllers/ReviewsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.Modules.Reviews.Api.Contracts;
using DoubleStar.Modules.Reviews.Application.Commands.ApproveReviewCommand;
using DoubleStar.Modules.Reviews.Application.Commands.DeleteReviewCommand;
using DoubleStar.Modules.Reviews.Application.Commands.RejectReviewCommand;
using DoubleStar.Modules.Reviews.Application.Commands.SubmitReviewCommand;
using DoubleStar.Modules.Reviews.Application.Queries.GetApprovedReviewsQuery;
using DoubleStar.Modules.Reviews.Application.Queries.GetPendingReviewsQuery;

namespace DoubleStar.Modules.Reviews.Api.Controllers;

[ApiController]
[Route("api/v1/reviews")]
public sealed class ReviewsController(ISender sender) : V1ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetApproved(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetApprovedReviewsQuery(), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPendingReviewsQuery(), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Submit(SubmitReviewRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SubmitReviewCommand(request.Rating, request.Comment), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ApproveReviewCommand(id), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Reject(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RejectReviewCommand(id), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteReviewCommand(id), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }
}