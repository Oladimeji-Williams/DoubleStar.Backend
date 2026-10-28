using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Customers.Application.Abstractions;
using DoubleStar.SharedKernel.Contracts.Customers;

namespace DoubleStar.Modules.Customers.Application.Queries.SearchCustomersQuery;

public sealed class SearchCustomersQueryHandler(
    ICustomerRepository customerRepository)
    : IRequestHandler<
        SearchCustomersQuery,
        Result<PagedResult<CustomerSummaryDto>>>
{
    public async Task<
        Result<PagedResult<CustomerSummaryDto>>>
        Handle(
            SearchCustomersQuery request,
            CancellationToken cancellationToken)
    {
        var result =
            await customerRepository.SearchAsync(
                request.Query,
                cancellationToken);

        var items =
            result.Items
                .Select(c =>
                    new CustomerSummaryDto(
                        c.Id,
                        c.Name,
                        c.Phone,
                        c.Email,
                        c.HasAccount))
                .ToList();

        return Result<
            PagedResult<CustomerSummaryDto>>
            .Success(
                new PagedResult<CustomerSummaryDto>(
                    items,
                    result.Page,
                    result.PageSize,
                    result.TotalCount));
    }
}