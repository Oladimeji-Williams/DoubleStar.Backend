// Application/Abstractions/ICustomerRepository.cs
using DoubleStar.Modules.Customers.Domain.Entities;

namespace DoubleStar.Modules.Customers.Application.Abstractions;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Customer?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> SearchAsync(string term, CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
}