// Application/Queries/GetTicketsByCustomerQuery/GetTicketsByCustomerQuery.cs
using DoubleStar.Modules.Repairs.Application.DTOs;

namespace DoubleStar.Modules.Repairs.Application.Queries.GetTicketsByCustomerQuery;

public sealed record GetTicketsByCustomerQuery(Guid CustomerId) : IRequest<Result<IReadOnlyList<RepairTicketDto>>>;