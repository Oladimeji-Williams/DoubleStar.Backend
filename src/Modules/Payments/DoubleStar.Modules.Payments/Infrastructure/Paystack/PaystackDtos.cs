// Infrastructure/Paystack/PaystackDtos.cs — shapes matching Paystack's JSON responses
using System.Text.Json.Serialization;

namespace DoubleStar.Modules.Payments.Infrastructure.Paystack;

internal sealed record PaystackInitializeResponse(
    bool Status, string Message, PaystackInitializeData? Data);

internal sealed record PaystackInitializeData(
    [property: JsonPropertyName("authorization_url")] string AuthorizationUrl,
    [property: JsonPropertyName("access_code")] string AccessCode,
    string Reference);

internal sealed record PaystackVerifyResponse(bool Status, string Message, PaystackVerifyData? Data);

internal sealed record PaystackVerifyData(string Status, long Amount, string Reference);