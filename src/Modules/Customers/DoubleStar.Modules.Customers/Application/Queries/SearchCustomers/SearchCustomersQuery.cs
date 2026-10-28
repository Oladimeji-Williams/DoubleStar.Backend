using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.SharedKernel.Contracts.Customers;

namespace DoubleStar.Modules.Customers.Application.Queries.SearchCustomersQuery;

public sealed record SearchCustomersQuery(
    PagedQuery Query)
    : IRequest<Result<PagedResult<CustomerSummaryDto>>>;