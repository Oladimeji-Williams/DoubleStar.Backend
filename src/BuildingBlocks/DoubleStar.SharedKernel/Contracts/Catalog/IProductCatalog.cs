// Contracts/Catalog/IProductCatalog.cs
namespace DoubleStar.SharedKernel.Contracts.Catalog;

public interface IProductCatalog
{
    Task<ProductSummaryDto?> GetByIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductSummaryDto>> GetByIdsAsync(IEnumerable<int> productIds, CancellationToken cancellationToken = default);
}