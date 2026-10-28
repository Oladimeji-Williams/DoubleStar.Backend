using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.DTOs;
using DoubleStar.Modules.Orders.Application.Mappings;

namespace DoubleStar.Modules.Orders.Application.Queries.GetMyOrders;
public sealed class GetMyOrdersQueryHandler(IOrderRepository orderRepository, ICurrentUser currentUser)
    : IRequestHandler<GetMyOrdersQuery, Result<PagedResult<OrderListDto>>>
{
    public async Task<Result<PagedResult<OrderListDto>>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid customerId)
        {
            return Result<PagedResult<OrderListDto>>.Failure(UserErrors.NotAuthenticated());
        }

        var result = await orderRepository.SearchForCustomerAsync(customerId, request.Query, cancellationToken);

        return Result<PagedResult<OrderListDto>>.Success(
            new PagedResult<OrderListDto>(
                result.Items.Select(o => o.ToListDto()).ToList(), result.Page, result.PageSize, result.TotalCount));
    }
}