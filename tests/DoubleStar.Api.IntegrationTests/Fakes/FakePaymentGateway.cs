using System.Collections.Concurrent;
using DoubleStar.SharedKernel.Abstractions.Payments;

namespace DoubleStar.Api.IntegrationTests.Fakes;

/// <summary>Stands in for Paystack: hands out fake checkout links and lets a test decide which references were "paid".</summary>
public sealed class FakePaymentGateway : IPaymentGateway
{
    private readonly ConcurrentDictionary<string, long> _amounts = new();
    private readonly ConcurrentDictionary<string, bool> _paid = new();

    public Task<PaymentInitializeResult> InitializeTransactionAsync(
        string email, long amountKobo, string reference, string callbackUrl, CancellationToken cancellationToken)
    {
        _amounts[reference] = amountKobo;
        return Task.FromResult(
            new PaymentInitializeResult($"https://paystack.test/pay/{reference}", $"access-{reference}", reference));
    }

    public Task<PaymentVerifyResult> VerifyTransactionAsync(string reference, CancellationToken cancellationToken)
    {
        var paid = _paid.ContainsKey(reference);
        return Task.FromResult(
            new PaymentVerifyResult(paid, _amounts.GetValueOrDefault(reference), reference, paid ? "success" : "abandoned"));
    }

    public bool VerifyWebhookSignature(string rawBody, string? signatureHeader) => true;

    /// <summary>Simulates the customer finishing payment on Paystack.</summary>
    public void MarkAsPaid(string reference) => _paid[reference] = true;
}