// Contracts/Customers/CustomerSummaryDto.cs
namespace DoubleStar.SharedKernel.Contracts.Customers;

public sealed record CustomerSummaryDto(Guid Id, string Name, string? Phone, string? Email, bool HasAccount);