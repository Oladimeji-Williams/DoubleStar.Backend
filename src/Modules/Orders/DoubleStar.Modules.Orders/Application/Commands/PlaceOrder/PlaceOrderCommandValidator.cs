//
// The checkout saga. There is no distributed transaction (each module owns a DbContext), so every
// step that can fail after a side effect has an explicit compensation:
//
//   1. validate cart            -> nothing to undo
//   2. reserve stock            -> released on any later failure
//   3. save order (PendingPayment)
//   4. start Paystack payment   -> on failure: cancel order + release stock
//   5. attach reference, clear cart
//
// Payment completion is NOT handled here — it arrives later through the Paystack webhook (or the
// callback verify) as PaymentSucceededEvent. See PaymentSucceededEventHandler.
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Common.Options;
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.SharedKernel.Contracts.Orders;
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.DTOs;
using DoubleStar.Modules.Orders.Application.Errors;
using DoubleStar.Modules.Orders.Application.Mappings;
using DoubleStar.Modules.Orders.Application.Options;
using DoubleStar.Modules.Orders.Application.Services;
using DoubleStar.Modules.Orders.Domain.Entities;

namespace DoubleStar.Modules.Orders.Application.Commands.PlaceOrder;
public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.FulfillmentMethod).IsInEnum();
        RuleFor(x => x.ContactName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactPhone).NotEmpty().MaximumLength(20)
            .Matches(@"^\+?[0-9\s\-]{7,20}$").WithMessage("Enter a valid phone number.");
        RuleFor(x => x.Notes).MaximumLength(500);
        RuleFor(x => x.IdempotencyKey).MaximumLength(100);

        When(x => x.FulfillmentMethod == FulfillmentMethod.Delivery, () =>
        {
            RuleFor(x => x.DeliveryAddress).NotEmpty().MaximumLength(500);
            RuleFor(x => x.DeliveryCity).NotEmpty().MaximumLength(100);
            RuleFor(x => x.DeliveryState).NotEmpty().MaximumLength(100);
        });
    }
}