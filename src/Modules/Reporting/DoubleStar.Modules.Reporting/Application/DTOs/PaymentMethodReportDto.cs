namespace DoubleStar.Modules.Reporting.Application.DTOs;

public sealed record PaymentMethodReportDto(
    string PaymentMethod,
    int TransactionCount,
    long AmountKobo);