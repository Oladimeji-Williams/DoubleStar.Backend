using DoubleStar.SharedKernel.Contracts.Inventory;
using DoubleStar.Modules.Catalog.Application.Abstractions;
using DoubleStar.Modules.Catalog.Application.DTOs;
using DoubleStar.Modules.Catalog.Application.Errors;
using DoubleStar.Modules.Catalog.Application.Mappings;

namespace DoubleStar.Modules.Catalog.Application.Queries.GetProductByIdQuery;

public sealed class GetProductByIdQueryHandler(
    IProductRepository productRepository,
    IStockLevelReader stockLevelReader)
    : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            return Result<ProductDto>.Failure(CatalogErrors.NotFound(request.Id));
        }

        var dto = await product.ToDto().WithAvailabilityAsync(stockLevelReader, cancellationToken);
        return Result<ProductDto>.Success(dto);
    }
}
