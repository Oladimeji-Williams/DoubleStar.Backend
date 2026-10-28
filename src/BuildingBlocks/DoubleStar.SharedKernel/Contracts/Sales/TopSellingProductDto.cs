namespace DoubleStar.SharedKernel.Contracts.Sales;

public sealed record TopSellingProductDto(
    int ProductId,
    int QuantitySold,
    long RevenueKobo);