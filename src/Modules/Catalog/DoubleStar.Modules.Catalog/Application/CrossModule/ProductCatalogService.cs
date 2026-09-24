// Application/CrossModule/ProductCatalogService.cs
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.Modules.Catalog.Application.Abstractions;
using DoubleStar.Modules.Catalog.Application.Mappings;

namespace DoubleStar.Modules.Catalog.Application.CrossModule;

public sealed class ProductCatalogService(IProductRepository productRepository) : IProductCatalog
{
    public async Task<ProductSummaryDto?> GetByIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(productId, cancellationToken);
        return product?.ToSummaryDto();
    }

    public async Task<IReadOnlyList<ProductSummaryDto>> GetByIdsAsync(
        IEnumerable<int> productIds, CancellationToken cancellationToken = default)
    {
        var products = await productRepository.GetByIdsAsync(productIds, cancellationToken);
        return products.Select(p => p.ToSummaryDto()).ToList();
    }

    public async Task<IReadOnlyList<ProductSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);
        return products.Select(p => p.ToSummaryDto()).ToList();
    }
}