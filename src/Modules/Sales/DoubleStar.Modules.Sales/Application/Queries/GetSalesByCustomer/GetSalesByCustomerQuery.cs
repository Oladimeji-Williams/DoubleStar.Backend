// Application/Queries/GetSalesByCustomerQuery/GetSalesByCustomerQuery.cs
using DoubleStar.Modules.Sales.Application.DTOs;

namespace DoubleStar.Modules.Sales.Application.Queries.GetSalesByCustomerQuery;

public sealed record GetSalesByCustomerQuery(Guid CustomerId) : IRequest<Result<IReadOnlyList<SaleDto>>>;