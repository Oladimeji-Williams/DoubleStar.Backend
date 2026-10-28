using DoubleStar.SharedKernel.Domain;
using DoubleStar.SharedKernel.Contracts.Orders;

namespace DoubleStar.Modules.Orders.Domain.Entities;

public sealed class OrderStatusHistory : Entity
{
    public int OrderId { get; private set; }
    public OrderStatus FromStatus { get; private set; }
    public OrderStatus ToStatus { get; private set; }
    public string? Note { get; private set; }

    private OrderStatusHistory() { }

    internal static OrderStatusHistory Create(OrderStatus from, OrderStatus to, string? note) => new()
    {
        FromStatus = from,
        ToStatus = to,
        Note = note,
    };
}