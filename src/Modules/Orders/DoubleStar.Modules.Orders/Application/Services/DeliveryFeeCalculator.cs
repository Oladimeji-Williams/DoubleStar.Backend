// Application/Services/DeliveryFeeCalculator.cs — one rule, used by the summary AND by place-order, so they can never disagree
using Microsoft.Extensions.Options;
using DoubleStar.SharedKernel.Contracts.Orders;
using DoubleStar.Modules.Orders.Application.Options;

namespace DoubleStar.Modules.Orders.Application.Services;

public sealed class DeliveryFeeCalculator(IOptions<OrdersOptions> options)
{
    private readonly OrdersOptions _options = options.Value;

    public long Calculate(FulfillmentMethod method, long subtotalKobo)
    {
        if (method == FulfillmentMethod.Pickup)
        {
            return 0;
        }

        var qualifiesForFreeDelivery =
            _options.FreeDeliveryThresholdKobo > 0 && subtotalKobo >= _options.FreeDeliveryThresholdKobo;

        return qualifiesForFreeDelivery ? 0 : _options.DeliveryFeeKobo;
    }
}