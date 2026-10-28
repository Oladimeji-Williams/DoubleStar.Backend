// Application/Commands/UpdateOrderStatus/UpdateOrderStatus.cs — staff fulfilment progress
using DoubleStar.SharedKernel.Contracts.Orders;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.Errors;
using DoubleStar.Modules.Orders.Application.Mappings;

namespace DoubleStar.Modules.Orders.Application.Commands.UpdateOrderStatus;

public sealed record UpdateOrderStatusCommand(int OrderId, OrderStatus Status) : IRequest<Result>;
