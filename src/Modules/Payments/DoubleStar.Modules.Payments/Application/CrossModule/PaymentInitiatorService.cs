// Application/CrossModule/PaymentInitiatorService.cs — NEW
using DoubleStar.SharedKernel.Abstractions.Payments;
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Payments.Application.Abstractions;
using DoubleStar.Modules.Payments.Application.Errors;
using DoubleStar.Modules.Payments.Domain.Entities;

namespace DoubleStar.Modules.Payments.Application.CrossModule;

public sealed class PaymentInitiatorService(
    IPaymentTransactionRepository paymentTransactionRepository,
    IPaymentGateway paymentGateway)
    : IPaymentInitiator
{
    public async Task<Result<PaymentInitiationDto>> InitializeAsync(
        PaymentSourceType sourceType,
        int sourceId,
        long amountKobo,
        string email,
        string callbackUrl,
        CancellationToken cancellationToken = default)
    {
        // Unique and known before we call Paystack: Verify and the webhook look the payment up by it.
        var reference = $"DS-{sourceType}-{sourceId}-{Guid.NewGuid():N}"[..40];

        PaymentInitializeResult gatewayResult;
        try
        {
            gatewayResult = await paymentGateway.InitializeTransactionAsync(
                email, amountKobo, reference, callbackUrl, cancellationToken);
        }
        catch (Exception ex) when (ex is InvalidOperationException or HttpRequestException)
        {
            return Result<PaymentInitiationDto>.Failure(PaymentErrors.GatewayFailure(ex.Message));
        }

        var transaction = PaymentTransaction.StartPaystack(sourceType, sourceId, amountKobo, gatewayResult.Reference);
        await paymentTransactionRepository.AddAsync(transaction, cancellationToken);

        return Result<PaymentInitiationDto>.Success(
            new PaymentInitiationDto(transaction.Id, gatewayResult.AuthorizationUrl, gatewayResult.Reference));
    }
}
