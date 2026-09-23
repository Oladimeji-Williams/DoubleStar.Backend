// .../InitializePaystackPaymentCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Payments;
using DoubleStar.Modules.Payments.Application.Abstractions;
using DoubleStar.Modules.Payments.Application.Errors;
using DoubleStar.Modules.Payments.Domain.Entities;

namespace DoubleStar.Modules.Payments.Application.Commands.InitializePaystackPaymentCommand;

public sealed class InitializePaystackPaymentCommandHandler(
    IPaymentTransactionRepository paymentTransactionRepository, IPaymentGateway paymentGateway)
    : IRequestHandler<InitializePaystackPaymentCommand, Result<string>>
{
    public async Task<Result<string>> Handle(InitializePaystackPaymentCommand request, CancellationToken cancellationToken)
    {
        // Referenceable and unique before we even call Paystack, since we need
        // something to look the transaction up by later in Verify/Webhook.
        var reference = $"DS-{request.SourceType}-{request.SourceId}-{Guid.NewGuid():N}"[..40];

        PaymentInitializeResult gatewayResult;
        try
        {
            gatewayResult = await paymentGateway.InitializeTransactionAsync(
                request.Email, request.AmountKobo, reference, request.CallbackUrl, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return Result<string>.Failure(PaymentErrors.GatewayFailure(ex.Message));
        }

        var transaction = PaymentTransaction.StartPaystack(
            request.SourceType, request.SourceId, request.AmountKobo, gatewayResult.Reference);
        await paymentTransactionRepository.AddAsync(transaction, cancellationToken);

        return Result<string>.Success(gatewayResult.AuthorizationUrl);
    }
}