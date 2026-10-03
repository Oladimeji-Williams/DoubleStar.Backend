// .../SetProductImageCommandHandler.cs
using DoubleStar.Modules.Catalog.Application.Abstractions;
using DoubleStar.Modules.Catalog.Application.DTOs;
using DoubleStar.Modules.Catalog.Application.Errors;
using DoubleStar.Modules.Catalog.Application.Mappings;

namespace DoubleStar.Modules.Catalog.Application.Commands.SetProductImageCommand;

public sealed class SetProductImageCommandHandler(IProductRepository productRepository)
    : IRequestHandler<SetProductImageCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(SetProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null) return Result<ProductDto>.Failure(CatalogErrors.NotFound(request.ProductId));

        product.SetImage(request.ImageUrl);
        await productRepository.UpdateAsync(product, cancellationToken);
        return Result<ProductDto>.Success(product.ToDto());
    }
}