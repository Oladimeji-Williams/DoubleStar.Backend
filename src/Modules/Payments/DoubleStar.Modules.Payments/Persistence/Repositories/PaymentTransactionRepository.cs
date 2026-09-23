// Persistence/Repositories/PaymentTransactionRepository.cs
using Microsoft.EntityFrameworkCore;
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Payments.Application.Abstractions;
using DoubleStar.Modules.Payments.Domain.Entities;

namespace DoubleStar.Modules.Payments.Persistence.Repositories;

public sealed class PaymentTransactionRepository(PaymentsDbContext dbContext) : IPaymentTransactionRepository
{
    private IQueryable<PaymentTransaction> WithIncludes() => dbContext.PaymentTransactions.Include(p => p.Refunds);

    public Task<PaymentTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        WithIncludes().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<PaymentTransaction?> GetByPaystackReferenceAsync(
        string reference, CancellationToken cancellationToken = default) =>
        WithIncludes().FirstOrDefaultAsync(p => p.PaystackReference == reference, cancellationToken);

    public async Task<IReadOnlyList<PaymentTransaction>> GetForSourceAsync(
        PaymentSourceType sourceType, int sourceId, CancellationToken cancellationToken = default) =>
        await WithIncludes().Where(p => p.SourceType == sourceType && p.SourceId == sourceId)
            .OrderBy(p => p.CreatedAt).ToListAsync(cancellationToken);

    public async Task AddAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default)
    {
        await dbContext.PaymentTransactions.AddAsync(transaction, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default)
    {
        dbContext.PaymentTransactions.Update(transaction);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}