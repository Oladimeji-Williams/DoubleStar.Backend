// Contracts/Sales/ISalesHistory.cs
namespace DoubleStar.SharedKernel.Contracts.Sales;

public interface ISalesHistory
{
    Task<IReadOnlyList<SaleSummaryDto>> GetAllForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SaleSummaryDto>> GetAllInRangeAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
}