using DoubleStar.SharedKernel.Contracts.Payments;

namespace DoubleStar.Modules.Payments.Application.Commands.InitializePaystackPayment;

public sealed class InitializePaystackPaymentCommandHandler(IPaymentInitiator paymentInitiator)
    : IRequestHandler<InitializePaystackPaymentCommand, Result<string>>
{
    public async Task<Result<string>> Handle(InitializePaystackPaymentCommand request, CancellationToken cancellationToken)
    {
        var result = await paymentInitiator.InitializeAsync(
            request.SourceType, request.SourceId, request.AmountKobo, request.Email, request.CallbackUrl, cancellationToken);

        return result.IsFailure
            ? Result<string>.Failure(result.Errors)
            : Result<string>.Success(result.Value!.AuthorizationUrl);
    }
}
