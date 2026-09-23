// Api/Contracts/RecordManualPaymentRequest.cs
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Payments.Domain.Enums;

namespace DoubleStar.Modules.Payments.Api.Contracts;

public sealed record RecordManualPaymentRequest(
    PaymentSourceType SourceType, int SourceId, long AmountKobo, PaymentMethod Method);