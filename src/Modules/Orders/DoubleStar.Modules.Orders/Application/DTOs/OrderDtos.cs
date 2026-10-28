using DoubleStar.SharedKernel.Contracts.Orders;

namespace DoubleStar.Modules.Orders.Application.DTOs;

public sealed record OrderLineDto(
    int Id,
    int ProductId,
    string ProductName,
    string Sku,
    long UnitPriceKobo,
    int Quantity,
    long LineTotalKobo,
    IReadOnlyList<string> SerialNumbers);

public sealed record OrderTimelineEntryDto(OrderStatus From, OrderStatus To, string? Note, DateTime AtUtc);

public sealed record OrderDto(
    int Id,
    string OrderNumber,
    Guid CustomerId,
    OrderStatus Status,
    FulfillmentMethod FulfillmentMethod,
    long SubtotalKobo,
    long DeliveryFeeKobo,
    long TotalKobo,
    string ContactName,
    string ContactPhone,
    string? DeliveryAddress,
    string? DeliveryCity,
    string? DeliveryState,
    string? Notes,
    DateTime CreatedAtUtc,
    DateTime? PaidAtUtc,
    DateTime ExpiresAtUtc,
    bool CanPay,
    bool CanCancel,
    bool NeedsAttention,
    string? AttentionReason,
    IReadOnlyList<OrderStatus> NextStatuses,
    IReadOnlyList<OrderLineDto> Lines,
    IReadOnlyList<OrderTimelineEntryDto> Timeline);

public sealed record OrderListDto(
    int Id,
    string OrderNumber,
    OrderStatus Status,
    FulfillmentMethod FulfillmentMethod,
    long TotalKobo,
    int ItemCount,
    string ContactName,
    DateTime CreatedAtUtc,
    bool NeedsAttention);

public sealed record PlaceOrderResultDto(OrderDto Order, string AuthorizationUrl, string PaymentReference);

public sealed record PaymentInitiationResultDto(string AuthorizationUrl, string PaymentReference);