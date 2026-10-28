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
public sealed class PlaceOrderCommandHandler(
    ICartRepository cartRepository,
    IOrderRepository orderRepository,
    ICurrentUser currentUser,
    IIdentityService identityService,
    IProductCatalog productCatalog,
    OrderStockService orderStockService,
    DeliveryFeeCalculator deliveryFeeCalculator,
    IPaymentInitiator paymentInitiator,
    IOptions<OrdersOptions> ordersOptions,
    IOptions<FrontendOptions> frontendOptions,
    TimeProvider timeProvider,
    ILogger<PlaceOrderCommandHandler> logger)
    : IRequestHandler<PlaceOrderCommand, Result<PlaceOrderResultDto>>
{
    public async Task<Result<PlaceOrderResultDto>> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid customerId)
        {
            return Result<PlaceOrderResultDto>.Failure(UserErrors.NotAuthenticated());
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;

        // Idempotency: a double-click or a retried request returns the order that was already created.
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existing = await orderRepository.GetByIdempotencyKeyAsync(
                customerId, request.IdempotencyKey.Trim(), cancellationToken);

            if (existing is not null)
            {
                return await ResumeAsync(existing, customerId, now, cancellationToken);
            }
        }

        var cart = await cartRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        if (cart is null || cart.Items.Count == 0)
        {
            return Result<PlaceOrderResultDto>.Failure(OrderErrors.CartEmpty());
        }

        var products = (await productCatalog.GetByIdsAsync(cart.Items.Select(i => i.ProductId), cancellationToken))
            .ToDictionary(p => p.Id);

        foreach (var item in cart.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product) || product.IsArchived)
            {
                return Result<PlaceOrderResultDto>.Failure(OrderErrors.ProductUnavailable(item.ProductId));
            }
        }

        var profile = await identityService.GetProfileAsync(customerId, cancellationToken);
        if (profile is null || string.IsNullOrWhiteSpace(profile.Email))
        {
            return Result<PlaceOrderResultDto>.Failure(OrderErrors.EmailRequired());
        }

        // ---- 2. reserve stock (all-or-nothing)
        var stockRequests = cart.Items
            .Select(i =>
            {
                var p = products[i.ProductId];
                return new StockRequest(p.Id, p.Name, i.Quantity, p.TrackingMode == StockTrackingMode.Serialized);
            })
            .ToList();

        var reservation = await orderStockService.ReserveAsync(stockRequests, cancellationToken);
        if (reservation.IsFailure)
        {
            return Result<PlaceOrderResultDto>.Failure(reservation.Errors);
        }

        var reserved = reservation.Value!;

        // ---- 3. snapshot + persist the order
        var drafts = cart.Items.Select(i =>
        {
            var p = products[i.ProductId];
            var stock = reserved.First(r => r.ProductId == p.Id);
            return new OrderLineDraft(
                p.Id, p.Name, p.Sku, p.UnitPriceKobo, i.Quantity, stock.IsSerialized, stock.SerialNumbers);
        }).ToList();

        var subtotal = drafts.Sum(d => d.UnitPriceKobo * d.Quantity);
        var deliveryFee = deliveryFeeCalculator.Calculate(request.FulfillmentMethod, subtotal);

        Order order;
        try
        {
            order = Order.Place(
                customerId,
                request.FulfillmentMethod,
                request.ContactName,
                request.ContactPhone,
                request.DeliveryAddress,
                request.DeliveryCity,
                request.DeliveryState,
                request.Notes,
                request.IdempotencyKey,
                deliveryFee,
                now.AddMinutes(ordersOptions.Value.PaymentWindowMinutes),
                drafts);

            await orderRepository.AddAsync(order, cancellationToken);
        }
        catch
        {
            await orderStockService.ReleaseAsync(reserved, CancellationToken.None);
            throw; // UnhandledExceptionBehavior turns this into a clean failure result
        }

        // ---- 4. start the payment
        var initiation = await StartPaymentAsync(order, profile.Email!, cancellationToken);
        if (initiation.IsFailure)
        {
            // Compensate: nobody can pay for this order, so don't hold their stock hostage.
            await orderStockService.ReleaseAsync(order, CancellationToken.None);
            order.Cancel("Payment could not be started");
            await orderRepository.SaveAsync(order, CancellationToken.None);

            return Result<PlaceOrderResultDto>.Failure(initiation.Errors);
        }

        // ---- 5. attach the reference and empty the cart
        order.AttachPayment(initiation.Value!.Reference);
        await orderRepository.SaveAsync(order, cancellationToken);

        try
        {
            cart.Clear();
            await cartRepository.SaveAsync(cart, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // The order exists and is payable; a stale cart is an annoyance, not a failure.
            logger.LogWarning(ex, "Order {OrderId} was placed but the cart could not be cleared.", order.Id);
        }

        return Result<PlaceOrderResultDto>.Success(
            new PlaceOrderResultDto(order.ToDto(now), initiation.Value.AuthorizationUrl, initiation.Value.Reference));
    }

    private async Task<Result<PlaceOrderResultDto>> ResumeAsync(
        Order existing, Guid customerId, DateTime now, CancellationToken cancellationToken)
    {
        if (existing.Status != OrderStatus.PendingPayment || existing.IsExpired(now))
        {
            // Already paid / cancelled / expired: hand it back, there's nothing to pay.
            return Result<PlaceOrderResultDto>.Success(new PlaceOrderResultDto(existing.ToDto(now), string.Empty, string.Empty));
        }

        var profile = await identityService.GetProfileAsync(customerId, cancellationToken);
        if (profile is null || string.IsNullOrWhiteSpace(profile.Email))
        {
            return Result<PlaceOrderResultDto>.Failure(OrderErrors.EmailRequired());
        }

        var initiation = await StartPaymentAsync(existing, profile.Email!, cancellationToken);
        if (initiation.IsFailure)
        {
            return Result<PlaceOrderResultDto>.Failure(initiation.Errors);
        }

        existing.AttachPayment(initiation.Value!.Reference);
        await orderRepository.SaveAsync(existing, cancellationToken);

        return Result<PlaceOrderResultDto>.Success(
            new PlaceOrderResultDto(existing.ToDto(now), initiation.Value.AuthorizationUrl, initiation.Value.Reference));
    }

    private Task<Result<PaymentInitiationDto>> StartPaymentAsync(Order order, string email, CancellationToken cancellationToken)
    {
        var callbackUrl = $"{frontendOptions.Value.CustomerAppUrl.TrimEnd('/')}/checkout/callback?orderId={order.Id}";

        return paymentInitiator.InitializeAsync(
            PaymentSourceType.Order, order.Id, order.TotalKobo, email, callbackUrl, cancellationToken);
    }
}