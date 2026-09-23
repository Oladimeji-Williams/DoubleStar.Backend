// Abstractions/Payments/IPaymentGateway.cs
namespace DoubleStar.SharedKernel.Abstractions.Payments;

public sealed record PaymentInitializeResult(string AuthorizationUrl, string AccessCode, string Reference);
public sealed record PaymentVerifyResult(bool Success, long AmountKobo, string Reference);

public interface IPaymentGateway
{
    Task<PaymentInitializeResult> InitializeTransactionAsync(
        string email, long amountKobo, string reference, string callbackUrl, CancellationToken cancellationToken);

    Task<PaymentVerifyResult> VerifyTransactionAsync(string reference, CancellationToken cancellationToken);

    bool VerifyWebhookSignature(string rawBody, string? signatureHeader);
}