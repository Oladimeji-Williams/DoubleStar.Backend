// Application/Queries/GetCustomerByIdQuery/GetCustomerByIdQuery.cs
using DoubleStar.SharedKernel.Contracts.Customers;

namespace DoubleStar.Modules.Customers.Application.Queries.GetCustomerByIdQuery;

public sealed record GetCustomerByIdQuery(Guid Id) : IRequest<Result<CustomerSummaryDto>>;