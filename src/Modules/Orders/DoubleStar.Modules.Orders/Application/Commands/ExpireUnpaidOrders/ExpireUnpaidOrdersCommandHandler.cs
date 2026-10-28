// Run every minute by ExpireUnpaidOrdersService. Frees stock held by orders nobody paid for.
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DoubleStar.SharedKernel.Contracts.Orders;
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.Mappings;
using DoubleStar.Modules.Orders.Application.Options;
using DoubleStar.Modules.Orders.Application.Services;

namespace DoubleStar.Modules.Orders.Application.Commands.ExpireUnpaidOrders;
public sealed class ExpireUnpaidOrdersCommandHandler(
    IOrderRepository orderRepository,
    OrderStockService orderStockService,
    IPaymentVerifier paymentVerifier,
    IPublisher publisher,
    IOptions<OrdersOptions> options,
    TimeProvider timeProvider,
    ILogger<ExpireUnpaidOrdersCommandHandler> logger)
    : IRequestHandler<ExpireUnpaidOrdersCommand, Result<int>>
{
    private const int BatchSize = 50;

    public async Task<Result<int>> Handle(ExpireUnpaidOrdersCommand request, CancellationToken cancellationToken)
    {
        var cutoff = timeProvider.GetUtcNow().UtcDateTime.AddMinutes(-options.Value.ExpiryGraceMinutes);
        var candidates = await orderRepository.GetPendingExpiredBeforeAsync(cutoff, BatchSize, cancellationToken);

        var expired = 0;

        foreach (var order in candidates)
        {
            try
            {
                // Last chance: the webhook may have been lost. If it was actually paid, this flips the order
                // to Paid (same tracked instance) and we leave it alone.
                if (!string.IsNullOrEmpty(order.PaymentReference))
                {
                    await paymentVerifier.VerifyAsync(order.PaymentReference, cancellationToken);
                }

                if (order.Status != OrderStatus.PendingPayment)
                {
                    continue;
                }

                await orderStockService.ReleaseAsync(order, cancellationToken);

                var from = order.Status;
                order.Expire();
                await orderRepository.SaveAsync(order, cancellationToken);
                await publisher.Publish(order.ToStatusChangedEvent(from), cancellationToken);

                expired++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // One bad order must not block the rest of the batch; it will be retried next tick.
                logger.LogError(ex, "Failed to expire order {OrderId}.", order.Id);
            }
        }

        if (expired > 0)
        {
            logger.LogInformation("Expired {Count} unpaid order(s).", expired);
        }

        return Result<int>.Success(expired);
    }
}