namespace DoubleStar.SharedKernel.Contracts.Payments;

public interface IPaymentRecordReader
{
    Task<IReadOnlyList<PaymentSummaryDto>> GetForSourceAsync(
        PaymentSourceType sourceType, int sourceId, CancellationToken cancellationToken = default);
}