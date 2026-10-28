using DoubleStar.Modules.Catalog.Application.DTOs;

namespace DoubleStar.Modules.Catalog.Application.Queries.GetAllBrandsQuery;

public sealed record GetAllBrandsQuery : IRequest<Result<IReadOnlyList<BrandDto>>>;