// Register this URL in the Paystack dashboard: https://<api-host>/api/v1/PaystackWebhook/webhook
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DoubleStar.SharedKernel.Abstractions.Payments;
using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.Modules.Payments.Application.Commands.VerifyPaystackPayment;

namespace DoubleStar.Modules.Payments.Api.Controllers;

/// <summary>
/// Paystack calls this directly — no JWT, so it authenticates itself via the HMAC signature.
/// Reads the raw body because the signature is computed over the exact bytes Paystack sent.
/// Status codes matter: anything but 2xx makes Paystack retry (which is what we want for our own
/// transient failures, and what we must NOT cause for events that aren't ours).
/// </summary>
[AllowAnonymous]
public sealed class PaystackWebhookController(
    ISender sender,
    IPaymentGateway paymentGateway,
    ILogger<PaystackWebhookController> logger) : V1ControllerBase
{
    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook(CancellationToken cancellationToken)
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);
        Request.Body.Position = 0;

        var signature = Request.Headers["x-paystack-signature"].ToString();
        if (!paymentGateway.VerifyWebhookSignature(rawBody, signature))
        {
            logger.LogWarning("Rejected Paystack webhook with an invalid signature.");
            return Unauthorized();
        }

        string? eventName;
        string? reference;

        try
        {
            using var document = JsonDocument.Parse(rawBody);
            var root = document.RootElement;

            eventName = root.TryGetProperty("event", out var eventElement) ? eventElement.GetString() : null;

            reference = root.TryGetProperty("data", out var data) && data.TryGetProperty("reference", out var referenceElement)
                ? referenceElement.GetString()
                : null;
        }
        catch (JsonException)
        {
            return BadRequest();
        }

        // We only act on successful charges. Acknowledge everything else so Paystack stops retrying.
        if (!string.Equals(eventName, "charge.success", StringComparison.Ordinal))
        {
            return Ok();
        }

        if (string.IsNullOrWhiteSpace(reference))
        {
            return BadRequest();
        }

        // Idempotent by design: verifying an already-settled payment just re-publishes PaymentSucceededEvent,
        // and subscribers are idempotent. A duplicate or retried delivery is therefore always safe.
        var result = await sender.Send(new VerifyPaystackPaymentCommand(reference), cancellationToken);

        if (result.IsSuccess)
        {
            return Ok();
        }

        if (result.Errors.Any(e => e.Code == "Payments.ReferenceNotFound"))
        {
            // Not one of ours (e.g. another integration on the same Paystack account). Retrying never helps.
            logger.LogWarning("Webhook for unknown reference {Reference} acknowledged and ignored.", reference);
            return Ok();
        }

        // Our own transient failure (gateway unreachable, database, subscriber) -> 5xx => Paystack retries.
        logger.LogError("Webhook processing failed for {Reference}: {Errors}", reference,
            string.Join("; ", result.Errors.Select(e => e.Code)));

        return StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
}
