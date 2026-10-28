using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using DoubleStar.SharedKernel.Abstractions.Payments;

namespace DoubleStar.Modules.Payments.Infrastructure.Paystack;

internal sealed class PaystackService(HttpClient httpClient, IOptions<PaystackOptions> options) : IPaymentGateway
{
    private readonly PaystackOptions _options = options.Value;

    public async Task<PaymentInitializeResult> InitializeTransactionAsync(
        string email, long amountKobo, string reference, string callbackUrl, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("transaction/initialize", new
        {
            email,
            amount = amountKobo,
            reference,
            callback_url = callbackUrl,
        }, cancellationToken);

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<PaystackInitializeResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Paystack returned an empty initialize response.");

        if (!body.Status || body.Data is null)
        {
            throw new InvalidOperationException($"Paystack initialize failed: {body.Message}");
        }

        return new PaymentInitializeResult(body.Data.AuthorizationUrl, body.Data.AccessCode, body.Data.Reference);
    }

    public async Task<PaymentVerifyResult> VerifyTransactionAsync(string reference, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"transaction/verify/{Uri.EscapeDataString(reference)}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<PaystackVerifyResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Paystack returned an empty verify response.");

        var status = body.Data?.Status ?? string.Empty;
        var success = body.Status && string.Equals(status, "success", StringComparison.OrdinalIgnoreCase);

        return new PaymentVerifyResult(success, body.Data?.Amount ?? 0, reference, status);
    }

    public bool VerifyWebhookSignature(string rawBody, string? signatureHeader)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader))
        {
            return false;
        }

        byte[] provided;
        try
        {
            provided = Convert.FromHexString(signatureHeader.Trim());
        }
        catch (FormatException)
        {
            return false;
        }

        var computed = HMACSHA512.HashData(Encoding.UTF8.GetBytes(_options.SecretKey), Encoding.UTF8.GetBytes(rawBody));

        return CryptographicOperations.FixedTimeEquals(computed, provided);
    }
}
