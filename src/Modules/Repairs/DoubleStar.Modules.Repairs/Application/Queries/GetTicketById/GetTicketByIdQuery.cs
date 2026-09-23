// Application/Queries/GetTicketByIdQuery/GetTicketByIdQuery.cs
using DoubleStar.Modules.Repairs.Application.DTOs;

namespace DoubleStar.Modules.Repairs.Application.Queries.GetTicketByIdQuery;

public sealed record GetTicketByIdQuery(int Id) : IRequest<Result<RepairTicketDto>>;