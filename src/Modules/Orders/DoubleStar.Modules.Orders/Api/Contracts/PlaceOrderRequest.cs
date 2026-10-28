using DoubleStar.SharedKernel.Contracts.Orders;

namespace DoubleStar.Modules.Orders.Api.Contracts;

public sealed record PlaceOrderRequest(
    FulfillmentMethod FulfillmentMethod,
    string ContactName,
    string ContactPhone,
    string? DeliveryAddress,
    string? DeliveryCity,
    string? DeliveryState,
    string? Notes,
    string? IdempotencyKey);
