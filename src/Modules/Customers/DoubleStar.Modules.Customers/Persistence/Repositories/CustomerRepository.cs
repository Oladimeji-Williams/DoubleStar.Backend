// Persistence/Repositories/CustomerRepository.cs
using Microsoft.EntityFrameworkCore;
using DoubleStar.Modules.Customers.Application.Abstractions;
using DoubleStar.Modules.Customers.Domain.Entities;

namespace DoubleStar.Modules.Customers.Persistence.Repositories;

public sealed class CustomerRepository(CustomersDbContext dbContext) : ICustomerRepository
{
    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Customer?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default) =>
        dbContext.Customers.FirstOrDefaultAsync(c => c.Phone == phone, cancellationToken);

    public async Task<IReadOnlyList<Customer>> SearchAsync(string term, CancellationToken cancellationToken = default)
    {
        var pattern = $"%{term}%";
        return await dbContext.Customers
            .Where(c => EF.Functions.ILike(c.Name, pattern) || (c.Phone != null && EF.Functions.ILike(c.Phone, pattern)))
            .OrderBy(c => c.Name)
            .Take(50)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await dbContext.Customers.AddAsync(customer, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        dbContext.Customers.Update(customer);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}