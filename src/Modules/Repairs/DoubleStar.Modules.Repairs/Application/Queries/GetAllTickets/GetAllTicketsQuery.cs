// Application/Queries/GetAllTicketsQuery/GetAllTicketsQuery.cs
using DoubleStar.Modules.Repairs.Application.DTOs;

namespace DoubleStar.Modules.Repairs.Application.Queries.GetAllTicketsQuery;

public sealed record GetAllTicketsQuery : IRequest<Result<IReadOnlyList<RepairTicketDto>>>;