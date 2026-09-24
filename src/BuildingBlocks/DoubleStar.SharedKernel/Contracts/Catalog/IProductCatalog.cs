// SharedKernel/Contracts/Catalog/IProductCatalog.cs — replace the whole file
namespace DoubleStar.SharedKernel.Contracts.Catalog;

public interface IProductCatalog
{
    Task<ProductSummaryDto?> GetByIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductSummaryDto>> GetByIdsAsync(IEnumerable<int> productIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
}