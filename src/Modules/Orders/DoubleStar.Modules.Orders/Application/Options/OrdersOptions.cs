// Application/Options/OrdersOptions.cs — bind from env:  Orders__DeliveryFeeKobo=250000
namespace DoubleStar.Modules.Orders.Application.Options;

public sealed class OrdersOptions
{
    public const string SectionName = "Orders";

    /// <summary>Flat delivery fee, in kobo. 200_000 = ₦2,000.</summary>
    public long DeliveryFeeKobo { get; init; } = 200_000;

    /// <summary>Subtotals at or above this get free delivery. 0 disables the offer.</summary>
    public long FreeDeliveryThresholdKobo { get; init; }

    /// <summary>How long stock stays reserved for an unpaid order.</summary>
    public int PaymentWindowMinutes { get; init; } = 30;

    /// <summary>Extra time the sweeper waits (a late webhook can still arrive) before expiring an order.</summary>
    public int ExpiryGraceMinutes { get; init; } = 5;

    public string PickupAddress { get; init; } = "10, Double Star Plaza, Ikorodu Garage, Ikorodu, Lagos";
}