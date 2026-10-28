using Microsoft.Extensions.Options;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.SharedKernel.Contracts.Orders;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.DTOs;
using DoubleStar.Modules.Orders.Application.Options;
using DoubleStar.Modules.Orders.Application.Services;

namespace DoubleStar.Modules.Orders.Application.Queries.GetCheckoutSummary;

public sealed class GetCheckoutSummaryQueryHandler(
    ICartRepository cartRepository,
    ICurrentUser currentUser,
    CartViewBuilder cartViewBuilder,
    DeliveryFeeCalculator deliveryFeeCalculator,
    IOptions<OrdersOptions> options)
    : IRequestHandler<GetCheckoutSummaryQuery, Result<CheckoutSummaryDto>>
{
    public async Task<Result<CheckoutSummaryDto>> Handle(GetCheckoutSummaryQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid customerId)
        {
            return Result<CheckoutSummaryDto>.Failure(UserErrors.NotAuthenticated());
        }

        var cart = await cartRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        var cartView = cart is null ? CartViewBuilder.Empty : await cartViewBuilder.BuildAsync(cart, cancellationToken);

        var fee = deliveryFeeCalculator.Calculate(request.FulfillmentMethod, cartView.SubtotalKobo);

        return Result<CheckoutSummaryDto>.Success(new CheckoutSummaryDto(
            cartView,
            request.FulfillmentMethod,
            fee,
            cartView.SubtotalKobo + fee,
            options.Value.PickupAddress,
            options.Value.PaymentWindowMinutes));
    }
}