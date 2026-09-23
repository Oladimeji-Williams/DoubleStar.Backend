// Application/Queries/GetAllSalesQuery/GetAllSalesQuery.cs
using DoubleStar.Modules.Sales.Application.DTOs;

namespace DoubleStar.Modules.Sales.Application.Queries.GetAllSalesQuery;

public sealed record GetAllSalesQuery : IRequest<Result<IReadOnlyList<SaleDto>>>;