using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.SharedKernel.Contracts.Orders;
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.Errors;
using DoubleStar.Modules.Orders.Application.Mappings;
using DoubleStar.Modules.Orders.Application.Services;

namespace DoubleStar.Modules.Orders.Application.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    ICurrentUser currentUser,
    OrderStockService orderStockService,
    IPaymentVerifier paymentVerifier,
    IPublisher publisher)
    : IRequestHandler<CancelOrderCommand, Result>
{
    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
        {
            return Result.Failure(UserErrors.NotAuthenticated());
        }

        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null || (!currentUser.IsStaff && order.CustomerId != userId))
        {
            return Result.Failure(OrderErrors.NotFound(request.OrderId));
        }

        if (!order.CanBeCancelled)
        {
            return Result.Failure(OrderErrors.InvalidTransition(
                $"An order that is {order.Status} can't be cancelled here."));
        }

        // The customer may have just paid in another tab. Ask the gateway before throwing the order away;
        // if it was paid, the event handler flips this same tracked order to Paid and we stop.
        if (!string.IsNullOrEmpty(order.PaymentReference))
        {
            await paymentVerifier.VerifyAsync(order.PaymentReference, cancellationToken);

            if (!order.CanBeCancelled)
            {
                return Result.Failure(OrderErrors.InvalidTransition(
                    "This order was just paid for, so it can't be cancelled. Contact us if you need a refund."));
            }
        }

        await orderStockService.ReleaseAsync(order, cancellationToken);

        var from = order.Status;
        order.Cancel(currentUser.IsStaff ? "Cancelled by staff" : "Cancelled by customer");
        await orderRepository.SaveAsync(order, cancellationToken);

        await publisher.Publish(order.ToStatusChangedEvent(from), cancellationToken);

        return Result.Success();
    }
}