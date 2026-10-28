// Application/CrossModule/PaymentVerifierService.cs — NEW
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Payments.Application.Commands.VerifyPaystackPayment;

namespace DoubleStar.Modules.Payments.Application.CrossModule;

public sealed class PaymentVerifierService(ISender sender) : IPaymentVerifier
{
    public async Task<Result<PaymentSummaryDto>> VerifyAsync(
        string reference, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new VerifyPaystackPaymentCommand(reference), cancellationToken);

        if (result.IsFailure)
        {
            return Result<PaymentSummaryDto>.Failure(result.Errors);
        }

        var payment = result.Value!;

        return Result<PaymentSummaryDto>.Success(
            new PaymentSummaryDto(
                payment.Id, payment.SourceType, payment.SourceId, payment.AmountKobo, payment.Method, payment.Status));
    }
}
