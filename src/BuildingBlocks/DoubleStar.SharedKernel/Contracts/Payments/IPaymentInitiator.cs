using DoubleStar.SharedKernel.Common.Primitives;

namespace DoubleStar.SharedKernel.Contracts.Payments;

public interface IPaymentInitiator
{
    Task<Result<PaymentInitiationDto>> InitializeAsync(
        PaymentSourceType sourceType,
        int sourceId,
        long amountKobo,
        string email,
        string callbackUrl,
        CancellationToken cancellationToken = default);
}
