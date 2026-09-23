// Application/CrossModule/PaymentRecordReaderService.cs
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Payments.Application.Abstractions;
using DoubleStar.Modules.Payments.Application.Mappings;

namespace DoubleStar.Modules.Payments.Application.CrossModule;

public sealed class PaymentRecordReaderService(IPaymentTransactionRepository paymentTransactionRepository)
    : IPaymentRecordReader
{
    public async Task<IReadOnlyList<PaymentSummaryDto>> GetForSourceAsync(
        PaymentSourceType sourceType, int sourceId, CancellationToken cancellationToken = default)
    {
        var transactions = await paymentTransactionRepository.GetForSourceAsync(sourceType, sourceId, cancellationToken);
        return transactions.Select(t => t.ToSummaryDto()).ToList();
    }
}