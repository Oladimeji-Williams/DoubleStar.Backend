namespace DoubleStar.SharedKernel.Contracts.Sales;

public sealed record SaleSummaryDto(int Id, Guid? CustomerId, long TotalKobo, DateTime CompletedAtUtc);