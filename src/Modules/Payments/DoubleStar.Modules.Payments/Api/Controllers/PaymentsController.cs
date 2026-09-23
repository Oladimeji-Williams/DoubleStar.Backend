// Api/Controllers/PaymentsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Payments.Api.Contracts;
using DoubleStar.Modules.Payments.Application.Commands.InitializePaystackPaymentCommand;
using DoubleStar.Modules.Payments.Application.Commands.RecordManualPaymentCommand;
using DoubleStar.Modules.Payments.Application.Commands.RefundPaymentCommand;
using DoubleStar.Modules.Payments.Application.Queries.GetPaymentByIdQuery;
using DoubleStar.Modules.Payments.Application.Queries.GetPaymentsBySourceQuery;

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
}