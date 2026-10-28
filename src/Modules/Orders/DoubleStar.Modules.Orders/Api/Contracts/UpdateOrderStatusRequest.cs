using DoubleStar.SharedKernel.Contracts.Orders;

namespace DoubleStar.Modules.Orders.Api.Contracts;

public sealed record UpdateOrderStatusRequest(OrderStatus Status);