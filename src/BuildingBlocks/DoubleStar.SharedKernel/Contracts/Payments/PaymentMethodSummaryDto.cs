namespace DoubleStar.SharedKernel.Contracts.Payments;

public sealed record PaymentMethodSummaryDto(
    string PaymentMethod,
    int TransactionCount,
    long AmountKobo);