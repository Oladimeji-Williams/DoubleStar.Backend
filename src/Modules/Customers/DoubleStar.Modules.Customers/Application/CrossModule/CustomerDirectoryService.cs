// Application/CrossModule/CustomerDirectoryService.cs
using DoubleStar.SharedKernel.Contracts.Customers;
using DoubleStar.Modules.Customers.Application.Abstractions;
using DoubleStar.Modules.Customers.Domain.Entities;

namespace DoubleStar.Modules.Customers.Application.CrossModule;

public sealed class CustomerDirectoryService(ICustomerRepository customerRepository) : ICustomerDirectory
{
    public async Task<CustomerSummaryDto?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);
        return customer is null ? null : ToSummary(customer);
    }

    public async Task<CustomerSummaryDto> GetOrCreateWalkInAsync(
        string name, string? phone, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(phone))
        {
            var existing = await customerRepository.GetByPhoneAsync(phone, cancellationToken);
            if (existing is not null)
            {
                return ToSummary(existing);
            }
        }

        var walkIn = Customer.CreateWalkIn(name, phone, email: null);
        await customerRepository.AddAsync(walkIn, cancellationToken);
        return ToSummary(walkIn);
    }

    private static CustomerSummaryDto ToSummary(Customer customer) =>
        new(customer.Id, customer.Name, customer.Phone, customer.Email, customer.HasAccount);
}