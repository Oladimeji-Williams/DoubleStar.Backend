using DoubleStar.SharedKernel.Common.Primitives;

namespace DoubleStar.SharedKernel.Contracts.Payments;

public interface IPaymentVerifier
{
    Task<Result<PaymentSummaryDto>> VerifyAsync(
        string reference,
        CancellationToken cancellationToken = default);
}
