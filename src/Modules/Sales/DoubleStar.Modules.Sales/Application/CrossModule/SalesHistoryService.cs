// Application/CrossModule/SalesHistoryService.cs
using DoubleStar.SharedKernel.Contracts.Sales;
using DoubleStar.Modules.Sales.Application.Abstractions;

namespace DoubleStar.Modules.Sales.Application.CrossModule;

public sealed class SalesHistoryService(ISaleRepository saleRepository) : ISalesHistory
{
    public async Task<IReadOnlyList<SaleSummaryDto>> GetAllForCustomerAsync(
        Guid customerId, CancellationToken cancellationToken = default)
    {
        var sales = await saleRepository.GetAllForCustomerAsync(customerId, cancellationToken);
        return sales.Select(ToSummary).ToList();
    }

    public async Task<IReadOnlyList<SaleSummaryDto>> GetAllInRangeAsync(
        DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var sales = await saleRepository.GetAllInRangeAsync(fromUtc, toUtc, cancellationToken);
        return sales.Select(ToSummary).ToList();
    }

    private static SaleSummaryDto ToSummary(Domain.Entities.Sale sale) =>
        new(sale.Id, sale.CustomerId, sale.TotalKobo, sale.CreatedAt);
}