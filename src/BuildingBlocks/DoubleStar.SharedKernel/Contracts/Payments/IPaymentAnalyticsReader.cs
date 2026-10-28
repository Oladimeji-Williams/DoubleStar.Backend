namespace DoubleStar.SharedKernel.Contracts.Payments;

public interface IPaymentAnalyticsReader
{
    Task<IReadOnlyList<PaymentMethodSummaryDto>>
        GetPaymentMethodSummaryAsync(
            DateTime fromUtc,
            DateTime toUtc,
            CancellationToken cancellationToken = default);
}