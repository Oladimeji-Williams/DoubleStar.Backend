// .../GetArchivedProductsQueryHandler.cs
using DoubleStar.Modules.Catalog.Application.Abstractions;
using DoubleStar.Modules.Catalog.Application.DTOs;
using DoubleStar.Modules.Catalog.Application.Mappings;

namespace DoubleStar.Modules.Catalog.Application.Queries.GetArchivedProductsQuery;

public sealed class GetArchivedProductsQueryHandler(IProductRepository productRepository)
    : IRequestHandler<GetArchivedProductsQuery, Result<IReadOnlyList<ProductDto>>>
{
    public async Task<Result<IReadOnlyList<ProductDto>>> Handle(GetArchivedProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetArchivedAsync(cancellationToken);
        return Result<IReadOnlyList<ProductDto>>.Success(products.Select(p => p.ToDto()).ToList());
    }
}