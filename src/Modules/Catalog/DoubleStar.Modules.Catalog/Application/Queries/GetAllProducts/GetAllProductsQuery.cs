using DoubleStar.Modules.Catalog.Application.DTOs;

namespace DoubleStar.Modules.Catalog.Application.Queries.GetAllProductsQuery;

public sealed record GetAllProductsQuery : IRequest<Result<IReadOnlyList<ProductDto>>>;