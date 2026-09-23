// .../CreateProductCommandHandler.cs
using DoubleStar.Modules.Catalog.Application.Abstractions;
using DoubleStar.Modules.Catalog.Application.DTOs;
using DoubleStar.Modules.Catalog.Application.Errors;
using DoubleStar.Modules.Catalog.Application.Mappings;
using DoubleStar.Modules.Catalog.Domain.Entities;

namespace DoubleStar.Modules.Catalog.Application.Commands.CreateProductCommand;

public sealed class CreateProductCommandHandler(IProductRepository productRepository)
    : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (await productRepository.GetBySkuAsync(request.Sku, cancellationToken) is not null)
        {
            return Result<ProductDto>.Failure(CatalogErrors.SkuAlreadyExists(request.Sku));
        }

        var product = Product.Create(
            request.Name, request.Sku, request.Description, request.CategoryId, request.BrandId,
            request.UnitPriceKobo, request.TrackingMode);

        await productRepository.AddAsync(product, cancellationToken);

        return Result<ProductDto>.Success(product.ToDto());
    }
}