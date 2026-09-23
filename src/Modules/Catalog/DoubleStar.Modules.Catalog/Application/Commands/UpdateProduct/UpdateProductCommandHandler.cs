// .../UpdateProductCommandHandler.cs
using DoubleStar.Modules.Catalog.Application.Abstractions;
using DoubleStar.Modules.Catalog.Application.Errors;

namespace DoubleStar.Modules.Catalog.Application.Commands.UpdateProductCommand;

public sealed class UpdateProductCommandHandler(IProductRepository productRepository)
    : IRequestHandler<UpdateProductCommand, Result>
{
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            return Result.Failure(CatalogErrors.NotFound(request.Id));
        }

        product.UpdateDetails(request.Name, request.Description, request.CategoryId, request.BrandId);
        await productRepository.UpdateAsync(product, cancellationToken);

        return Result.Success();
    }
}