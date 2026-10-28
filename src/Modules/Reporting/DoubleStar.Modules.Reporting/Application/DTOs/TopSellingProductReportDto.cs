namespace DoubleStar.Modules.Reporting.Application.DTOs;

public sealed record TopSellingProductReportDto(
    int ProductId,
    string ProductName,
    int QuantitySold,
    long RevenueKobo);