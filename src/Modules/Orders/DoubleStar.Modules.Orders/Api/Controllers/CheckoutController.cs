// Api/Controllers/CheckoutController.cs — /api/v1/checkout
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.SharedKernel.Contracts.Orders;
using DoubleStar.Modules.Orders.Api.Contracts;
using DoubleStar.Modules.Orders.Application.Commands.PlaceOrder;
using DoubleStar.Modules.Orders.Application.Commands.VerifyOrderPayment;
using DoubleStar.Modules.Orders.Application.Queries.GetCheckoutSummary;

namespace DoubleStar.Modules.Orders.Api.Controllers;

[Authorize(Roles = "Customer")]
public sealed class CheckoutController(ISender sender) : V1ControllerBase
{
    /// <summary>Totals (incl. delivery fee) and cart issues for the chosen fulfilment method.</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] FulfillmentMethod fulfillmentMethod = FulfillmentMethod.Pickup,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetCheckoutSummaryQuery(fulfillmentMethod), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    /// <summary>
    /// Reserves stock, creates the order and starts the Paystack payment. Returns the URL to redirect to.
    /// Send a stable IdempotencyKey per checkout attempt so retries can't create duplicate orders.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PlaceOrder(PlaceOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new PlaceOrderCommand(
            request.FulfillmentMethod,
            request.ContactName,
            request.ContactPhone,
            request.DeliveryAddress,
            request.DeliveryCity,
            request.DeliveryState,
            request.Notes,
            request.IdempotencyKey);

        var result = await sender.Send(command, cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpPost("{orderId:int}/verify")]
    public async Task<IActionResult> VerifyPayment(int orderId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new VerifyOrderPaymentCommand(orderId), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }
}