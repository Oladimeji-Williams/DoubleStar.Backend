// Catalog/Application/Queries/GetArchivedProductsQuery/GetArchivedProductsQuery.cs
using DoubleStar.Modules.Catalog.Application.DTOs;

namespace DoubleStar.Modules.Catalog.Application.Queries.GetArchivedProductsQuery;

public sealed record GetArchivedProductsQuery : IRequest<Result<IReadOnlyList<ProductDto>>>;