// Application/Queries/GetProductByIdQuery/GetProductByIdQuery.cs
using DoubleStar.Modules.Catalog.Application.DTOs;

namespace DoubleStar.Modules.Catalog.Application.Queries.GetProductByIdQuery;

public sealed record GetProductByIdQuery(int Id) : IRequest<Result<ProductDto>>;