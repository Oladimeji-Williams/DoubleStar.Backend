using DoubleStar.Modules.Payments.Application.Abstractions;
using DoubleStar.SharedKernel.Contracts.Payments;

namespace DoubleStar.Modules.Payments.Application.CrossModule;

public sealed class PaymentAnalyticsReaderService(
    IPaymentTransactionRepository paymentTransactionRepository)
    : IPaymentAnalyticsReader
{
    public Task<IReadOnlyList<PaymentMethodSummaryDto>>
        GetPaymentMethodSummaryAsync(
            DateTime fromUtc,
            DateTime toUtc,
            CancellationToken cancellationToken = default)
    {
        return paymentTransactionRepository
            .GetPaymentMethodSummaryAsync(
                fromUtc,
                toUtc,
                cancellationToken);
    }
}