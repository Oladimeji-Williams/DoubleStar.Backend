namespace DoubleStar.SharedKernel.Abstractions.Payments;

public sealed record PaymentInitializeResult(string AuthorizationUrl, string AccessCode, string Reference);

/// <summary>Status is the gateway's own word: success | failed | abandoned | ongoing | pending | reversed.</summary>
public sealed record PaymentVerifyResult(bool Success, long AmountKobo, string Reference, string Status = "");

public interface IPaymentGateway
{
    Task<PaymentInitializeResult> InitializeTransactionAsync(
        string email, long amountKobo, string reference, string callbackUrl, CancellationToken cancellationToken);

    Task<PaymentVerifyResult> VerifyTransactionAsync(string reference, CancellationToken cancellationToken);

    bool VerifyWebhookSignature(string rawBody, string? signatureHeader);
}
