// Application/Queries/GetAllCategoriesQuery/GetAllCategoriesQuery.cs
namespace DoubleStar.Modules.Catalog.Application.Queries.GetAllCategoriesQuery;

public sealed record CategoryDto(int Id, string Name);
public sealed record GetAllCategoriesQuery : IRequest<Result<IReadOnlyList<CategoryDto>>>;