using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Payments.Api.Contracts;
using DoubleStar.Modules.Payments.Application.Commands.InitializePaystackPayment;
using DoubleStar.Modules.Payments.Application.Commands.RecordManualPayment;
using DoubleStar.Modules.Payments.Application.Commands.RefundPayment;
using DoubleStar.Modules.Payments.Application.Queries.GetPaymentById;
using DoubleStar.Modules.Payments.Application.Queries.GetPaymentsBySource;
using DoubleStar.Modules.Payments.Application.Queries.GetPagedPayments;
using System.Text.Json;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Filtering;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;

namespace DoubleStar.Modules.Payments.Api.Controllers;

[Authorize(Roles = "Admin,Manager,Cashier")]
public sealed class PaymentsController(ISender sender) : V1ControllerBase
{
    [HttpPost("paystack/initialize")]
    public async Task<IActionResult> InitializePaystack(InitializePaystackPaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new InitializePaystackPaymentCommand(
                request.SourceType, request.SourceId, request.AmountKobo, request.Email, request.CallbackUrl),
            cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpPost("manual")]
    public async Task<IActionResult> RecordManual(RecordManualPaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RecordManualPaymentCommand(request.SourceType, request.SourceId, request.AmountKobo, request.Method),
            cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpPost("{id:guid}/refund")]
    public async Task<IActionResult> Refund(Guid id, RefundPaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RefundPaymentCommand(id, request.AmountKobo, request.Reason), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPaymentByIdQuery(id), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpGet("source/{sourceType}/{sourceId:int}")]
    public async Task<IActionResult> GetBySource(
        PaymentSourceType sourceType, int sourceId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPaymentsBySourceQuery(sourceType, sourceId), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
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
                new GetPagedPaymentsQuery(query),
                cancellationToken);

        return result.IsFailure
            ? Failure(result)
            : Success(result.Value);
    }
}