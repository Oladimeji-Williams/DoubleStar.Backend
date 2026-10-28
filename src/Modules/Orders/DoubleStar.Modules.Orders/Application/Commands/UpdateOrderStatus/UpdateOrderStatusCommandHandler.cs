// Application/Commands/UpdateOrderStatus/UpdateOrderStatus.cs — staff fulfilment progress
using DoubleStar.SharedKernel.Contracts.Orders;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.Errors;
using DoubleStar.Modules.Orders.Application.Mappings;

namespace DoubleStar.Modules.Orders.Application.Commands.UpdateOrderStatus;
public sealed class UpdateOrderStatusCommandHandler(IOrderRepository orderRepository, IPublisher publisher)
    : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound(request.OrderId));
        }

        var from = order.Status;

        try
        {
            order.Advance(request.Status);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(OrderErrors.InvalidTransition(ex.Message));
        }

        await orderRepository.SaveAsync(order, cancellationToken);
        await publisher.Publish(order.ToStatusChangedEvent(from), cancellationToken);

        return Result.Success();
    }
}