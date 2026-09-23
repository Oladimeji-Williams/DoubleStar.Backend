// Api/Contracts/InitializePaystackPaymentRequest.cs
using DoubleStar.SharedKernel.Contracts.Payments;

namespace DoubleStar.Modules.Payments.Api.Contracts;

public sealed record InitializePaystackPaymentRequest(
    PaymentSourceType SourceType, int SourceId, long AmountKobo, string Email, string CallbackUrl);