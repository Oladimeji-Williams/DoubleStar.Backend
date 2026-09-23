// Application/Queries/GetSaleByIdQuery/GetSaleByIdQuery.cs
using DoubleStar.Modules.Sales.Application.DTOs;

namespace DoubleStar.Modules.Sales.Application.Queries.GetSaleByIdQuery;

public sealed record GetSaleByIdQuery(int Id) : IRequest<Result<SaleDto>>;