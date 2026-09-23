// Application/DTOs/PaymentTransactionDto.cs
using DoubleStar.SharedKernel.Contracts.Payments;

namespace DoubleStar.Modules.Payments.Application.DTOs;

public sealed record PaymentTransactionDto(
    Guid Id, PaymentSourceType SourceType, int SourceId, long AmountKobo, long TotalRefundedKobo,
    string Method, string Status, string? PaystackReference);