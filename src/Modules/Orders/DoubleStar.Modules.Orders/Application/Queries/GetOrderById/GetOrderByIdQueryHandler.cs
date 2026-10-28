using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.DTOs;
using DoubleStar.Modules.Orders.Application.Errors;
using DoubleStar.Modules.Orders.Application.Mappings;

namespace DoubleStar.Modules.Orders.Application.Queries.GetOrderById;
public sealed class GetOrderByIdQueryHandler(
    IOrderRepository orderRepository,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
        {
            return Result<OrderDto>.Failure(UserErrors.NotAuthenticated());
        }

        var order = await orderRepository.GetByIdAsync(request.Id, cancellationToken);

        // Customers only ever see their own orders; anyone else gets the same answer as "doesn't exist".
        if (order is null || (!currentUser.IsStaff && order.CustomerId != userId))
        {
            return Result<OrderDto>.Failure(OrderErrors.NotFound(request.Id));
        }

        return Result<OrderDto>.Success(order.ToDto(timeProvider.GetUtcNow().UtcDateTime));
    }
}