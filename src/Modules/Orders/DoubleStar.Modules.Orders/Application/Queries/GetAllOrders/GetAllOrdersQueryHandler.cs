// Application/Queries/GetAllOrders/GetAllOrders.cs — staff
using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.DTOs;
using DoubleStar.Modules.Orders.Application.Mappings;

namespace DoubleStar.Modules.Orders.Application.Queries.GetAllOrders;
public sealed class GetAllOrdersQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetAllOrdersQuery, Result<PagedResult<OrderListDto>>>
{
    public async Task<Result<PagedResult<OrderListDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var result = await orderRepository.SearchAsync(request.Query, cancellationToken);

        return Result<PagedResult<OrderListDto>>.Success(
            new PagedResult<OrderListDto>(
                result.Items.Select(o => o.ToListDto()).ToList(), result.Page, result.PageSize, result.TotalCount));
    }
}