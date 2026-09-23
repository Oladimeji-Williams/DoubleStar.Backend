// .../SetProductPriceCommandHandler.cs
using DoubleStar.Modules.Catalog.Application.Abstractions;
using DoubleStar.Modules.Catalog.Application.Errors;

namespace DoubleStar.Modules.Catalog.Application.Commands.SetProductPriceCommand;

public sealed class SetProductPriceCommandHandler(IProductRepository productRepository)
    : IRequestHandler<SetProductPriceCommand, Result>
{
    public async Task<Result> Handle(SetProductPriceCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            return Result.Failure(CatalogErrors.NotFound(request.Id));
        }

        product.SetPrice(request.UnitPriceKobo);
        await productRepository.UpdateAsync(product, cancellationToken);

        return Result.Success();
    }
}