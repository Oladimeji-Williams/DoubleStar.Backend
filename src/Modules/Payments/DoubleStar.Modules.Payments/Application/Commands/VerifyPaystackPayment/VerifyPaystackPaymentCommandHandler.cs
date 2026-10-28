  // Why this behaves the way it does:
  //  * Paystack reporting "pending/ongoing" is NOT a failure. A customer returning to the callback early
  //    must not permanently kill a payment that later succeeds.
  //  * A Successful payment re-publishes PaymentSucceededEvent on every verify, so a subscriber that
  //    failed once still gets the event on the next webhook retry (at-least-once; subscribers must be idempotent).
using Microsoft.Extensions.Logging;
using DoubleStar.SharedKernel.Abstractions.Payments;
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Payments.Application.Abstractions;
using DoubleStar.Modules.Payments.Application.DTOs;
using DoubleStar.Modules.Payments.Application.Errors;
using DoubleStar.Modules.Payments.Application.Mappings;
using DoubleStar.Modules.Payments.Domain.Entities;
using DoubleStar.Modules.Payments.Domain.Enums;

namespace DoubleStar.Modules.Payments.Application.Commands.VerifyPaystackPayment;

public sealed class VerifyPaystackPaymentCommandHandler(
    IPaymentTransactionRepository paymentTransactionRepository,
    IPaymentGateway paymentGateway,
    IPublisher publisher,
    ILogger<VerifyPaystackPaymentCommandHandler> logger)
    : IRequestHandler<VerifyPaystackPaymentCommand, Result<PaymentTransactionDto>>
{
    private static readonly string[] TerminalFailureStatuses = ["failed", "abandoned", "reversed"];

    public async Task<Result<PaymentTransactionDto>> Handle(
        VerifyPaystackPaymentCommand request, CancellationToken cancellationToken)
    {
        var transaction = await paymentTransactionRepository.GetByPaystackReferenceAsync(
            request.Reference, cancellationToken);

        if (transaction is null)
        {
            return Result<PaymentTransactionDto>.Failure(PaymentErrors.ReferenceNotFound(request.Reference));
        }

        if (transaction.Status != PaymentStatus.Pending)
        {
            if (transaction.Status == PaymentStatus.Successful)
            {
                await PublishSucceededAsync(transaction, cancellationToken);
            }

            return Result<PaymentTransactionDto>.Success(transaction.ToDto());
        }

        PaymentVerifyResult verification;
        try
        {
            verification = await paymentGateway.VerifyTransactionAsync(request.Reference, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException)
        {
            logger.LogWarning(ex, "Paystack verify failed for {Reference}", request.Reference);
            return Result<PaymentTransactionDto>.Failure(PaymentErrors.GatewayFailure(ex.Message));
        }

        if (verification.Success && verification.AmountKobo == transaction.AmountKobo)
        {
            transaction.MarkSuccessful();
            await paymentTransactionRepository.UpdateAsync(transaction, cancellationToken);
            await PublishSucceededAsync(transaction, cancellationToken);
        }
        else if (verification.Success)
        {
            // Money moved but not the amount we asked for. Never fulfil — needs a human.
            logger.LogCritical(
                "Amount mismatch on {Reference}: expected {Expected} kobo, gateway reported {Actual} kobo.",
                request.Reference, transaction.AmountKobo, verification.AmountKobo);

            transaction.MarkFailed();
            await paymentTransactionRepository.UpdateAsync(transaction, cancellationToken);
        }
        else if (TerminalFailureStatuses.Contains(verification.Status, StringComparer.OrdinalIgnoreCase))
        {
            transaction.MarkFailed();
            await paymentTransactionRepository.UpdateAsync(transaction, cancellationToken);
        }
        // Otherwise (ongoing / pending): leave it Pending; the webhook or the next poll will settle it.

        return Result<PaymentTransactionDto>.Success(transaction.ToDto());
    }

    private Task PublishSucceededAsync(PaymentTransaction transaction, CancellationToken cancellationToken) =>
        publisher.Publish(
            new PaymentSucceededEvent(
                transaction.Id,
                transaction.SourceType,
                transaction.SourceId,
                transaction.AmountKobo,
                transaction.PaystackReference ?? string.Empty),
            cancellationToken);
}
