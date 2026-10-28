using DoubleStar.BuildingBlocks.Application.Common.Queries.Filtering;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Payments.Domain.Entities;

namespace DoubleStar.Modules.Payments.Application.Abstractions;

public interface IPaymentTransactionRepository
{
    Task<PaymentTransaction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PaymentTransaction?> GetByPaystackReferenceAsync(
        string reference,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentTransaction>> GetForSourceAsync(
        PaymentSourceType sourceType,
        int sourceId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<PaymentTransaction>> GetPagedAsync(
        PagedQuery query,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        PaymentTransaction transaction,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        PaymentTransaction transaction,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentMethodSummaryDto>>
        GetPaymentMethodSummaryAsync(
            DateTime fromUtc,
            DateTime toUtc,
            CancellationToken cancellationToken = default);
}