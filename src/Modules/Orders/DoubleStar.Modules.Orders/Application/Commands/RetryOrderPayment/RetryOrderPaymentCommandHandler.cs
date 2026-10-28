// "Complete payment" on an order that is still PendingPayment (closed the Paystack tab, card declined, ...).
// A fresh Paystack reference is created per attempt; the window is extended so a retry isn't instantly expired.
using Microsoft.Extensions.Options;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Common.Options;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.DTOs;
using DoubleStar.Modules.Orders.Application.Errors;
using DoubleStar.Modules.Orders.Application.Options;

namespace DoubleStar.Modules.Orders.Application.Commands.RetryOrderPayment;

public sealed class RetryOrderPaymentCommandHandler(
    IOrderRepository orderRepository,
    ICurrentUser currentUser,
    IIdentityService identityService,
    IPaymentInitiator paymentInitiator,
    IOptions<OrdersOptions> ordersOptions,
    IOptions<FrontendOptions> frontendOptions,
    TimeProvider timeProvider)
    : IRequestHandler<RetryOrderPaymentCommand, Result<PaymentInitiationResultDto>>
{
    public async Task<Result<PaymentInitiationResultDto>> Handle(
        RetryOrderPaymentCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid customerId)
        {
            return Result<PaymentInitiationResultDto>.Failure(UserErrors.NotAuthenticated());
        }

        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        // Someone else's order looks exactly like a missing one.
        if (order is null || order.CustomerId != customerId)
        {
            return Result<PaymentInitiationResultDto>.Failure(OrderErrors.NotFound(request.OrderId));
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;

        if (order.Status != DoubleStar.SharedKernel.Contracts.Orders.OrderStatus.PendingPayment)
        {
            return Result<PaymentInitiationResultDto>.Failure(OrderErrors.NotPayable());
        }

        if (order.IsExpired(now))
        {
            return Result<PaymentInitiationResultDto>.Failure(OrderErrors.PaymentWindowElapsed());
        }

        var profile = await identityService.GetProfileAsync(customerId, cancellationToken);
        if (profile is null || string.IsNullOrWhiteSpace(profile.Email))
        {
            return Result<PaymentInitiationResultDto>.Failure(OrderErrors.EmailRequired());
        }

        var callbackUrl = $"{frontendOptions.Value.CustomerAppUrl.TrimEnd('/')}/checkout/callback?orderId={order.Id}";

        var initiation = await paymentInitiator.InitializeAsync(
            PaymentSourceType.Order, order.Id, order.TotalKobo, profile.Email!, callbackUrl, cancellationToken);

        if (initiation.IsFailure)
        {
            return Result<PaymentInitiationResultDto>.Failure(initiation.Errors);
        }

        order.AttachPayment(initiation.Value!.Reference);
        order.ExtendPaymentWindow(now.AddMinutes(ordersOptions.Value.PaymentWindowMinutes));
        await orderRepository.SaveAsync(order, cancellationToken);

        return Result<PaymentInitiationResultDto>.Success(
            new PaymentInitiationResultDto(initiation.Value.AuthorizationUrl, initiation.Value.Reference));
    }
}