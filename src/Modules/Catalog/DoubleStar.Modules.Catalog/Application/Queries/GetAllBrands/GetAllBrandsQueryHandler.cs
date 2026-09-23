// .../GetAllBrandsQueryHandler.cs
using DoubleStar.Modules.Catalog.Application.Abstractions;

namespace DoubleStar.Modules.Catalog.Application.Queries.GetAllBrandsQuery;

public sealed class GetAllBrandsQueryHandler(IBrandRepository brandRepository)
    : IRequestHandler<GetAllBrandsQuery, Result<IReadOnlyList<BrandDto>>>
{
    public async Task<Result<IReadOnlyList<BrandDto>>> Handle(GetAllBrandsQuery request, CancellationToken cancellationToken)
    {
        var brands = await brandRepository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<BrandDto>>.Success(brands.Select(b => new BrandDto(b.Id, b.Name)).ToList());
    }
}