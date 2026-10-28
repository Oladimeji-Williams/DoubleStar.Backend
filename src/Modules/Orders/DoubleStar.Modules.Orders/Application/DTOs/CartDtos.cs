using DoubleStar.SharedKernel.Contracts.Orders;

namespace DoubleStar.Modules.Orders.Application.DTOs;

public sealed record CartItemDto(
    int ProductId,
    string Name,
    string Sku,
    string? ImageUrl,
    long UnitPriceKobo,
    int Quantity,
    long LineTotalKobo,
    int? AvailableQuantity,
    bool IsAvailable,
    string? Issue);

public sealed record CartDto(
    IReadOnlyList<CartItemDto> Items,
    int ItemCount,
    long SubtotalKobo,
    bool HasIssues);

public sealed record CheckoutSummaryDto(
    CartDto Cart,
    FulfillmentMethod FulfillmentMethod,
    long DeliveryFeeKobo,
    long TotalKobo,
    string PickupAddress,
    int PaymentWindowMinutes);