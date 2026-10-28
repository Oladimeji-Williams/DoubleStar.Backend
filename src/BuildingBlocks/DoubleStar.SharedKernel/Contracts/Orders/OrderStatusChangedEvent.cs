using MediatR;

namespace DoubleStar.SharedKernel.Contracts.Orders;

public sealed record OrderStatusChangedEvent(
    int OrderId,
    string OrderNumber,
    Guid CustomerId,
    OrderStatus FromStatus,
    OrderStatus ToStatus,
    long TotalKobo) : INotification;
