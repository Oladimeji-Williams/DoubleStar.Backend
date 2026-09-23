// Contracts/Payments/PaymentSummaryDto.cs
namespace DoubleStar.SharedKernel.Contracts.Payments;

public sealed record PaymentSummaryDto(
    Guid Id, PaymentSourceType SourceType, int SourceId, long AmountKobo, string Method, string Status);