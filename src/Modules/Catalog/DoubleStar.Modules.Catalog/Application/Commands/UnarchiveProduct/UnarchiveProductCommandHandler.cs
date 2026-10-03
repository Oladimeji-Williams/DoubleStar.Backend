// .../UnarchiveProductCommandHandler.cs
using DoubleStar.Modules.Catalog.Application.Abstractions;
using DoubleStar.Modules.Catalog.Application.Errors;

namespace DoubleStar.Modules.Catalog.Application.Commands.UnarchiveProductCommand;

public sealed class UnarchiveProductCommandHandler(IProductRepository productRepository)
    : IRequestHandler<UnarchiveProductCommand, Result>
{
    public async Task<Result> Handle(UnarchiveProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null) return Result.Failure(CatalogErrors.NotFound(request.Id));

        product.Unarchive();
        await productRepository.UpdateAsync(product, cancellationToken);
        return Result.Success();
    }
}