// .../GetCustomerByIdQueryHandler.cs
using DoubleStar.SharedKernel.Contracts.Customers;
using DoubleStar.Modules.Customers.Application.Abstractions;
using DoubleStar.Modules.Customers.Application.Errors;

namespace DoubleStar.Modules.Customers.Application.Queries.GetCustomerByIdQuery;

public sealed class GetCustomerByIdQueryHandler(ICustomerRepository customerRepository)
    : IRequestHandler<GetCustomerByIdQuery, Result<CustomerSummaryDto>>
{
    public async Task<Result<CustomerSummaryDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (customer is null)
        {
            return Result<CustomerSummaryDto>.Failure(CustomerErrors.NotFound(request.Id));
        }

        return Result<CustomerSummaryDto>.Success(
            new CustomerSummaryDto(customer.Id, customer.Name, customer.Phone, customer.Email, customer.HasAccount));
    }
}