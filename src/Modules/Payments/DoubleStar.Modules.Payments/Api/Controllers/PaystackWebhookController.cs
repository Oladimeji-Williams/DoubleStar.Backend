// Api/Controllers/PaystackWebhookController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoubleStar.SharedKernel.Abstractions.Payments;
using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.Modules.Payments.Application.Commands.VerifyPaystackPaymentCommand;
using Microsoft.AspNetCore.Http;

namespace DoubleStar.Modules.Payments.Api.Controllers;

/// <summary>
/// Paystack calls this directly — no JWT, so it authenticates itself via the
/// HMAC signature instead. Reads the raw body because the signature is
/// computed over the exact bytes Paystack sent, not a re-serialized model.
/// </summary>
[AllowAnonymous]
public sealed class PaystackWebhookController(ISender sender, IPaymentGateway paymentGateway) : V1ControllerBase
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
            return Unauthorized();
        }

        using var document = System.Text.Json.JsonDocument.Parse(rawBody);
        var reference = document.RootElement.GetProperty("data").GetProperty("reference").GetString();

        if (string.IsNullOrEmpty(reference))
        {
            return BadRequest();
        }

        // Idempotent by design — VerifyPaystackPaymentCommand short-circuits if
        // the transaction is already resolved, so a duplicate webhook delivery
        // (Paystack retries on anything but a 200) is always safe to replay.
        await sender.Send(new VerifyPaystackPaymentCommand(reference), cancellationToken);

        return Ok();
    }
}