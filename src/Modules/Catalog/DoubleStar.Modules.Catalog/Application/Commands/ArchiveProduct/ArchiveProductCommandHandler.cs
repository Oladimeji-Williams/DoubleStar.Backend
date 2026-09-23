// .../ArchiveProductCommandHandler.cs
using DoubleStar.Modules.Catalog.Application.Abstractions;
using DoubleStar.Modules.Catalog.Application.Errors;

namespace DoubleStar.Modules.Catalog.Application.Commands.ArchiveProductCommand;

public sealed class ArchiveProductCommandHandler(IProductRepository productRepository)
    : IRequestHandler<ArchiveProductCommand, Result>
{
    public async Task<Result> Handle(ArchiveProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            return Result.Failure(CatalogErrors.NotFound(request.Id));
        }

        product.Archive();
        await productRepository.UpdateAsync(product, cancellationToken);

        return Result.Success();
    }
}