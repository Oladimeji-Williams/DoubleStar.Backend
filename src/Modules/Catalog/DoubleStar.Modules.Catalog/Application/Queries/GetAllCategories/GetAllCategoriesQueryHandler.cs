// .../GetAllCategoriesQueryHandler.cs
using DoubleStar.Modules.Catalog.Application.Abstractions;

namespace DoubleStar.Modules.Catalog.Application.Queries.GetAllCategoriesQuery;

public sealed class GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<GetAllCategoriesQuery, Result<IReadOnlyList<CategoryDto>>>
{
    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(
        GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<CategoryDto>>.Success(
            categories.Select(c => new CategoryDto(c.Id, c.Name)).ToList());
    }
}