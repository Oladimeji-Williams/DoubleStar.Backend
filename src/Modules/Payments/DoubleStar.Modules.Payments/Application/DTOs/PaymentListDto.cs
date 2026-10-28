using DoubleStar.SharedKernel.Contracts.Payments;

namespace DoubleStar.Modules.Payments.Application.DTOs;

public sealed record PaymentListDto(
    Guid Id,
    PaymentSourceType SourceType,
    int SourceId,
    long AmountKobo,
    long TotalRefundedKobo,
    string Method,
    string Status,
    string? PaystackReference,
    DateTime CreatedAt);