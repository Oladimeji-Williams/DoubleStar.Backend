// .../SearchCustomersQueryHandler.cs
using DoubleStar.SharedKernel.Contracts.Customers;
using DoubleStar.Modules.Customers.Application.Abstractions;

namespace DoubleStar.Modules.Customers.Application.Queries.SearchCustomersQuery;

public sealed class SearchCustomersQueryHandler(ICustomerRepository customerRepository)
    : IRequestHandler<SearchCustomersQuery, Result<IReadOnlyList<CustomerSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<CustomerSummaryDto>>> Handle(
        SearchCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await customerRepository.SearchAsync(request.Term, cancellationToken);
        return Result<IReadOnlyList<CustomerSummaryDto>>.Success(
            customers.Select(c => new CustomerSummaryDto(c.Id, c.Name, c.Phone, c.Email, c.HasAccount)).ToList());
    }
}