using DoubleStar.Modules.Payments.Application.Abstractions;
using DoubleStar.Modules.Payments.Application.Errors;

namespace DoubleStar.Modules.Payments.Application.Commands.RefundPayment;

public sealed class RefundPaymentCommandHandler(IPaymentTransactionRepository paymentTransactionRepository)
    : IRequestHandler<RefundPaymentCommand, Result>
{
    public async Task<Result> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var transaction = await paymentTransactionRepository.GetByIdAsync(request.PaymentId, cancellationToken);
        if (transaction is null)
        {
            return Result.Failure(PaymentErrors.NotFound(request.PaymentId));
        }

        try
        {
            transaction.IssueRefund(request.AmountKobo, request.Reason);
        }
        catch (InvalidOperationException ex)
        {
            // Not refundable (still pending / failed) or more than is left to refund.
            return Result.Failure(PaymentErrors.InvalidState(ex.Message));
        }

        await paymentTransactionRepository.UpdateAsync(transaction, cancellationToken);
        return Result.Success();
    }
}