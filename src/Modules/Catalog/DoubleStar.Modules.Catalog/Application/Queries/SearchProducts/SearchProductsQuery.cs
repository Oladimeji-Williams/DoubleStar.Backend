// Application/Queries/SearchProductsQuery/SearchProductsQuery.cs
using DoubleStar.Modules.Catalog.Application.DTOs;

namespace DoubleStar.Modules.Catalog.Application.Queries.SearchProductsQuery;

public sealed record SearchProductsQuery(string Term) : IRequest<Result<IReadOnlyList<ProductDto>>>;