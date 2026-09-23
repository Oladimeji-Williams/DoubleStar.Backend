// .../CreateWalkInCustomerCommandHandler.cs
using DoubleStar.SharedKernel.Contracts.Customers;
using DoubleStar.Modules.Customers.Application.Abstractions;
using DoubleStar.Modules.Customers.Application.Errors;
using DoubleStar.Modules.Customers.Domain.Entities;

namespace DoubleStar.Modules.Customers.Application.Commands.CreateWalkInCustomerCommand;

public sealed class CreateWalkInCustomerCommandHandler(ICustomerRepository customerRepository)
    : IRequestHandler<CreateWalkInCustomerCommand, Result<CustomerSummaryDto>>
{
    public async Task<Result<CustomerSummaryDto>> Handle(
        CreateWalkInCustomerCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Phone) &&
            await customerRepository.GetByPhoneAsync(request.Phone, cancellationToken) is not null)
        {
            return Result<CustomerSummaryDto>.Failure(CustomerErrors.PhoneAlreadyInUse(request.Phone));
        }

        var customer = Customer.CreateWalkIn(request.Name, request.Phone, request.Email);
        await customerRepository.AddAsync(customer, cancellationToken);

        return Result<CustomerSummaryDto>.Success(
            new CustomerSummaryDto(customer.Id, customer.Name, customer.Phone, customer.Email, customer.HasAccount));
    }
}