// Called by the browser when Paystack redirects back. The webhook is the source of truth and usually
// wins the race; this just makes the UI feel instant and covers a delayed/missed webhook.
using Microsoft.Extensions.Logging;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.DTOs;
using DoubleStar.Modules.Orders.Application.Errors;
using DoubleStar.Modules.Orders.Application.Mappings;

namespace DoubleStar.Modules.Orders.Application.Commands.VerifyOrderPayment;
public sealed class VerifyOrderPaymentCommandHandler(
    IOrderRepository orderRepository,
    ICurrentUser currentUser,
    IPaymentVerifier paymentVerifier,
    TimeProvider timeProvider,
    ILogger<VerifyOrderPaymentCommandHandler> logger)
    : IRequestHandler<VerifyOrderPaymentCommand, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(VerifyOrderPaymentCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid customerId)
        {
            return Result<OrderDto>.Failure(UserErrors.NotAuthenticated());
        }

        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null || order.CustomerId != customerId)
        {
            return Result<OrderDto>.Failure(OrderErrors.NotFound(request.OrderId));
        }

        if (!string.IsNullOrEmpty(order.PaymentReference) &&
            order.Status == DoubleStar.SharedKernel.Contracts.Orders.OrderStatus.PendingPayment)
        {
            // On success this publishes PaymentSucceededEvent; the Orders subscriber updates THIS tracked instance.
            var verification = await paymentVerifier.VerifyAsync(order.PaymentReference, cancellationToken);

            if (verification.IsFailure)
            {
                // Gateway hiccup: not the customer's problem. Return the current state; the UI keeps polling.
                logger.LogWarning(
                    "Could not verify payment for order {OrderId}: {Errors}",
                    order.Id, string.Join("; ", verification.Errors.Select(e => e.Code)));
            }
        }

        return Result<OrderDto>.Success(order.ToDto(timeProvider.GetUtcNow().UtcDateTime));
    }
}