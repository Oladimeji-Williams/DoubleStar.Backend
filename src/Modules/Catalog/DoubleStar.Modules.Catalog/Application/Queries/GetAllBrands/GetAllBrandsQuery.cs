// Application/Queries/GetAllBrandsQuery/GetAllBrandsQuery.cs
namespace DoubleStar.Modules.Catalog.Application.Queries.GetAllBrandsQuery;

public sealed record BrandDto(int Id, string Name);
public sealed record GetAllBrandsQuery : IRequest<Result<IReadOnlyList<BrandDto>>>;