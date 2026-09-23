// Application/Queries/SearchCustomersQuery/SearchCustomersQuery.cs
using DoubleStar.SharedKernel.Contracts.Customers;

namespace DoubleStar.Modules.Customers.Application.Queries.SearchCustomersQuery;

public sealed record SearchCustomersQuery(string Term) : IRequest<Result<IReadOnlyList<CustomerSummaryDto>>>;