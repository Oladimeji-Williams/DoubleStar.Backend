// .../GetAllProductsQueryHandler.cs
using DoubleStar.Modules.Catalog.Application.Abstractions;
using DoubleStar.Modules.Catalog.Application.DTOs;
using DoubleStar.Modules.Catalog.Application.Mappings;

namespace DoubleStar.Modules.Catalog.Application.Queries.GetAllProductsQuery;

public sealed class GetAllProductsQueryHandler(IProductRepository productRepository)
    : IRequestHandler<GetAllProductsQuery, Result<IReadOnlyList<ProductDto>>>
{
    public async Task<Result<IReadOnlyList<ProductDto>>> Handle(
        GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<ProductDto>>.Success(products.Select(p => p.ToDto()).ToList());
    }
}