// .../CreateBrandCommandHandler.cs
using DoubleStar.Modules.Catalog.Application.Abstractions;
using DoubleStar.Modules.Catalog.Domain.Entities;

namespace DoubleStar.Modules.Catalog.Application.Commands.CreateBrandCommand;

public sealed class CreateBrandCommandHandler(IBrandRepository brandRepository)
    : IRequestHandler<CreateBrandCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = Brand.Create(request.Name);
        await brandRepository.AddAsync(brand, cancellationToken);
        return Result<int>.Success(brand.Id);
    }
}