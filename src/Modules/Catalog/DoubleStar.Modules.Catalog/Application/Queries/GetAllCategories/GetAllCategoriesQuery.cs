using DoubleStar.Modules.Catalog.Application.DTOs;

namespace DoubleStar.Modules.Catalog.Application.Queries.GetAllCategoriesQuery;

public sealed record GetAllCategoriesQuery : IRequest<Result<IReadOnlyList<CategoryDto>>>;