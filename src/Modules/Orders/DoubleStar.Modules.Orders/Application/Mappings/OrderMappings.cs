// Application/Mappings/OrderMappings.cs — manual, explicit, no mapping library
using DoubleStar.SharedKernel.Contracts.Orders;
using DoubleStar.Modules.Orders.Application.DTOs;
using DoubleStar.Modules.Orders.Domain.Entities;

namespace DoubleStar.Modules.Orders.Application.Mappings;

public static class OrderMappings
{
    public static OrderLineDto ToDto(this OrderLine line) => new(
        line.Id,
        line.ProductId,
        line.ProductName,
        line.Sku,
        line.UnitPriceKobo,
        line.Quantity,
        line.LineTotalKobo,
        line.SerialNumbers);

    public static OrderTimelineEntryDto ToDto(this OrderStatusHistory history) => new(
        history.FromStatus,
        history.ToStatus,
        history.Note,
        history.CreatedAt);

    public static OrderDto ToDto(this Order order, DateTime utcNow) => new(
        order.Id,
        order.OrderNumber,
        order.CustomerId,
        order.Status,
        order.FulfillmentMethod,
        order.SubtotalKobo,
        order.DeliveryFeeKobo,
        order.TotalKobo,
        order.ContactName,
        order.ContactPhone,
        order.DeliveryAddress,
        order.DeliveryCity,
        order.DeliveryState,
        order.Notes,
        order.CreatedAt,
        order.PaidAtUtc,
        order.ExpiresAtUtc,
        CanPay: order.Status == OrderStatus.PendingPayment && !order.IsExpired(utcNow),
        CanCancel: order.CanBeCancelled,
        NeedsAttention: order.NeedsAttention,
        AttentionReason: order.AttentionReason,
        NextStatuses: order.StaffNextStatuses,
        Lines: order.Lines.Select(l => l.ToDto()).ToList(),
        Timeline: order.StatusHistory.OrderBy(h => h.CreatedAt).Select(h => h.ToDto()).ToList());

    public static OrderListDto ToListDto(this Order order) => new(
        order.Id,
        order.OrderNumber,
        order.Status,
        order.FulfillmentMethod,
        order.TotalKobo,
        order.Lines.Sum(l => l.Quantity),
        order.ContactName,
        order.CreatedAt,
        order.NeedsAttention);

    public static OrderStatusChangedEvent ToStatusChangedEvent(this Order order, OrderStatus from) => new(
        order.Id,
        order.OrderNumber,
        order.CustomerId,
        from,
        order.Status,
        order.TotalKobo);
}