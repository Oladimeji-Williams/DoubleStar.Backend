// .../VerifyPaystackPaymentCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Payments;
using DoubleStar.Modules.Payments.Application.Abstractions;
using DoubleStar.Modules.Payments.Application.DTOs;
using DoubleStar.Modules.Payments.Application.Errors;
using DoubleStar.Modules.Payments.Application.Mappings;

namespace DoubleStar.Modules.Payments.Application.Commands.VerifyPaystackPaymentCommand;

public sealed class VerifyPaystackPaymentCommandHandler(
    IPaymentTransactionRepository paymentTransactionRepository, IPaymentGateway paymentGateway)
    : IRequestHandler<VerifyPaystackPaymentCommand, Result<PaymentTransactionDto>>
{
    public async Task<Result<PaymentTransactionDto>> Handle(
        VerifyPaystackPaymentCommand request, CancellationToken cancellationToken)
    {
        var transaction = await paymentTransactionRepository.GetByPaystackReferenceAsync(request.Reference, cancellationToken);
        if (transaction is null)
        {
            return Result<PaymentTransactionDto>.Failure(PaymentErrors.ReferenceNotFound(request.Reference));
        }

        // Already resolved (e.g. webhook beat us to it) — return the current
        // state instead of re-verifying or throwing on the status transition.
        if (transaction.Status.ToString() is not "Pending")
        {
            return Result<PaymentTransactionDto>.Success(transaction.ToDto());
        }

        var verifyResult = await paymentGateway.VerifyTransactionAsync(request.Reference, cancellationToken);

        if (verifyResult.Success && verifyResult.AmountKobo == transaction.AmountKobo)
        {
            transaction.MarkSuccessful();
        }
        else
        {
            transaction.MarkFailed();
        }

        await paymentTransactionRepository.UpdateAsync(transaction, cancellationToken);
        return Result<PaymentTransactionDto>.Success(transaction.ToDto());
    }
}