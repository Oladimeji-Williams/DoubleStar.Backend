// Api/Contracts/RefundPaymentRequest.cs
namespace DoubleStar.Modules.Payments.Api.Contracts;

public sealed record RefundPaymentRequest(long AmountKobo, string Reason);